using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Library.Models;
using System.Windows;
using Library.Utils;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.DB;
using System.Text;

namespace RoomStudies.Models
{
    public class RoomStudyModel : ModelBase
    {
        private readonly Room _room;
        private Autodesk.Revit.DB.Point _rotationCenter;
        private double _rotationAngle;
        private ViewSheet _viewSheet;
        private readonly double _offset;
        private bool _isRectangular = true;
        private List<BoundarySegment> _relevantSegments = [];

        public delegate void Notify(string message);
        public event Notify ErrorEvent;

        public RoomStudyModel(Room room)
        {
            _room = room ?? throw new System.ArgumentNullException(nameof(room));
            _offset = 0.5;
        }

        public string GetRoomNumber() => _room.Number;
        public string GetRoomName() => _room.Name;
        public string GetSheetNumber() => _viewSheet?.SheetNumber ?? "N/A";
        public string GetSheetName() => _viewSheet?.Name ?? "N/A";

        public void CreateRoomStudy(ElementId titleBlockId)
        {
            ModelTransaction = new(Doc);
            ModelTransaction.Start($"Processing room: {_room.Number} - {_room.Name}");

            _viewSheet = ViewSheet.Create(Doc, titleBlockId);
            if (_viewSheet == null) throw new System.InvalidOperationException("Sheet creation failed.");

            ExtractGeometricalData();

            if (_isRectangular)
            {
                Element elevationMarker = CreateElevationMarker(_rotationCenter.Coord, "Elevation", "Interior Elevation");
                if (elevationMarker is ElevationMarker marker)
                {
                    CreateElevationViews(marker);
                    RotateElement(elevationMarker, _rotationCenter.Coord, _rotationAngle);
                }
            }
            else
            {
                List<ViewSection> viewSections =  CreateSectionView();
                for (int i = 0; i < viewSections.Count(); i++)
                {
                    CreateViewPort(viewSections[i], new XYZ(i, i, 0));
                }

            }

            ViewPlan floorPlan = CreateViewPlan(ViewFamily.FloorPlan);
            ViewPlan ceilingPlan = CreateViewPlan(ViewFamily.CeilingPlan);

            CreateViewPort(floorPlan, XYZ.Zero);
            CreateViewPort(ceilingPlan, XYZ.Zero);

            SetSheetAttributes();
            ModelTransaction.Commit();
        }

        private void SetSheetAttributes()
        {
            if (_viewSheet == null) return;

            try
            {
                _viewSheet.LookupParameter("Sheet Name")?.Set(_room.Name);
                _viewSheet.LookupParameter("Sheet Number")?.Set(_room.Number);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting sheet attributes: {e.Message}");
                ModelTransaction.RollBack();
                throw new Exception("Error setting sheet attributes.");
            }
        }

        private void SetSectionAttributes(ViewSection section, int index)
        {
            if (index < 0)
                throw new System.ArgumentOutOfRangeException(nameof(index), "Index muss >= 0 sein.");

            var suffix = new StringBuilder();

            while (true)
            {
                int remainder = index % 26;

                char letter = (char)('A' + remainder);

                suffix.Insert(0, letter);

                index = (index / 26) - 1;

                if (index < 0)
                    break;
            }

            section.FindParameter(BuiltInParameter.VIEW_NAME)?.Set($"{_room.Number} - {suffix}");

            section.Scale = 20;
        }

        private void ExtractGeometricalData()
        {
            var boundarySegments = _room.GetBoundarySegments(new SpatialElementBoundaryOptions());
            (_rotationCenter, XYZ referenceVector) = CalculateCentroid(boundarySegments);
            _rotationAngle = CalculateAngle(referenceVector);
        }

        private (Autodesk.Revit.DB.Point, XYZ) CalculateCentroid(IList<IList<BoundarySegment>> segments)
        {
            double x = 0, y = 0, z = 0, maxLength = 0;
            int numSegments = 0;
            XYZ referenceVector = XYZ.Zero;
            double trheshold = UnitUtils.ConvertToInternalUnits(0.5, UnitTypeId.Meters);

            foreach (var segmentList in segments)
            {
                for (int i = 0; i < segmentList.Count(); i++)
                {
                    BoundarySegment currentBoundarySegment = segmentList[i];
                    numSegments++;
                    XYZ start = currentBoundarySegment.GetCurve().GetEndPoint(0);
                    XYZ end = currentBoundarySegment.GetCurve().GetEndPoint(1);

                    x += start.X;
                    y += start.Y;
                    z += start.Z;

                    if (currentBoundarySegment.GetCurve().ApproximateLength > maxLength)
                    {
                        maxLength = currentBoundarySegment.GetCurve().ApproximateLength;
                        referenceVector = new XYZ(end.X - start.X, end.Y - start.Y, 0);
                    }

                    if (currentBoundarySegment.GetCurve().ApproximateLength >= trheshold)
                    {
                        _relevantSegments.Add(currentBoundarySegment);
                    }



                    if (i+1 <= segmentList.Count() - 1)
                    {
                        BoundarySegment nextBoundarySegment = segmentList[i+1];
                        /*
                        if (!end.IsAlmostEqualTo(nextBoundarySegment.GetCurve().GetEndPoint(0)))
                        {
                            ModelTransaction.RollBack();
                            throw new Exception($"Boundary not valid:\nEnd: {end}\nStart {nextBoundarySegment.GetCurve().GetEndPoint(0)}");
                        }
                        */
                        double angleBetween = UnitUtils.Convert((end - start).AngleTo(nextBoundarySegment.GetCurve().GetEndPoint(1) - nextBoundarySegment.GetCurve().GetEndPoint(0)), UnitTypeId.Radians, UnitTypeId.Degrees);
                        if ((Math.Round(angleBetween, 2) != 90.00) && _isRectangular) // Check if the angle between two segments is not 90 degrees and hasnt be set to false before
                        {
                            _isRectangular = false;
                        }
                    }
                }
            }

            XYZ centroid = new(x / numSegments, y / numSegments, z / numSegments);
            return (Autodesk.Revit.DB.Point.Create(centroid), referenceVector);
        }

        private double CalculateAngle(XYZ referenceVector)
        {
            XYZ unitVector = XYZ.BasisX;
            return referenceVector.X < 0
                ? unitVector.AngleTo(referenceVector.Negate().Normalize())
                : unitVector.AngleTo(referenceVector.Normalize());
        }

        public ISet<ElementId> GetViewSet() => _viewSheet?.GetAllPlacedViews() ?? new HashSet<ElementId>();

        public IList<ElementId> GetAllElementsOnSheet()
        {
            if (_viewSheet == null) return [];

            var categories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Views,
                BuiltInCategory.OST_TitleBlocks,
                BuiltInCategory.OST_LegendComponents,
                BuiltInCategory.OST_Schedules
            };

            return _viewSheet.GetDependentElements(new ElementMulticategoryFilter(categories));
        }

        private Element CreateElevationMarker(XYZ point, string familyName, string typeName)
        {
            ElementId viewFamilyTypeId = GetViewTypeId(Doc, familyName, typeName);
            if (viewFamilyTypeId == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Error", $"Family '{familyName}' with type '{typeName}' not found.");
                return null;
            }

            return ElevationMarker.CreateElevationMarker(Doc, viewFamilyTypeId, point, 20);
        }

        private BoundingBoxXYZ CreateSectionBoundingBox(BoundarySegment segment)
        {
            Level level = _room.Level;
            double baseElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters) - _offset;
            Level upperLevel = Doc.GetElement(_room.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL).AsElementId()) as Level;
            double upperLevelElevation = UnitUtils.ConvertFromInternalUnits(upperLevel.Elevation, UnitTypeId.Meters);
            double upperOffset = _room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).AsDouble();
            double topElevation = upperLevelElevation + upperOffset + _offset;

            XYZ p0 = segment.GetCurve().GetEndPoint(0);
            XYZ p1 = segment.GetCurve().GetEndPoint(1);

            XYZ globalUp = XYZ.BasisZ;

            XYZ zDir = (p1 - p0).CrossProduct(globalUp).Normalize();
            XYZ xDir = (p1 - p0).Normalize();
            XYZ yDir = zDir.CrossProduct(xDir).Normalize();

            double dot = xDir.DotProduct(yDir.CrossProduct(zDir));
            if (dot <= 0)
            {
                xDir = xDir.Negate();
            }

            Transform t = Transform.Identity;
            t.BasisX = xDir;
            t.BasisY = yDir;
            t.BasisZ = zDir;
            t.Origin = p0;

            /*
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, p0);
            SketchPlane sketchPlane = SketchPlane.Create(Doc, plane);

            Doc.Create.NewModelCurve(segment.GetCurve(), sketchPlane);
            */

            double roomHeight = topElevation - baseElevation;
            double segmentLength = segment.GetCurve().ApproximateLength;

            BoundingBoxXYZ boundingBox = new();
            boundingBox.Transform = t;

            boundingBox.Min = new XYZ(0, - _offset, -1);
            boundingBox.Max = new XYZ(segmentLength, roomHeight, 1);

            boundingBox.Enabled = true;

            return boundingBox;

        }

        private List<ViewSection> CreateSectionView()
        {
            List<ViewSection> sectionViews = [];
            if (_relevantSegments.Count == 0)
            {
                TaskDialog.Show("Error", "No relevant segments found.");
                return null;
            }
            else
            {
                for (int i = 0; i<_relevantSegments.Count(); i++)
                {
                    BoundarySegment segment = _relevantSegments[i];
                    ElementId viewFamilyTypeId = GetViewTypeId(Doc, "Section", "Building Section");
                    BoundingBoxXYZ sectionBoundingBox = CreateSectionBoundingBox(segment);
                    Debug.WriteLine($"Min: {sectionBoundingBox.Min}\nMax: {sectionBoundingBox.Max}");
                    try
                    {
                        ViewSection viewSection = ViewSection.CreateSection(Doc, viewFamilyTypeId, sectionBoundingBox);
                        SetSectionAttributes(viewSection, i);
                        sectionViews.Add(viewSection);
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException AE)
                    {
                        ModelTransaction.RollBack();
                        throw new System.ArgumentException(AE.Message);
                    }

                }
                return sectionViews;
            }
        }

        private void CreateElevationViews(ElevationMarker elevationMarker)
        {
            for (int i = 0; i < 4; i++)
            {
                View view = elevationMarker.CreateElevation(Doc, Doc.ActiveView.Id, i);
                if (view != null)
                {
                    Viewport.Create(Doc, _viewSheet.Id, view.Id, new XYZ(i, i, 0));
                    // TransformCropRegion(view);
                }
            }
        }

        private Viewport CreateViewPort(View view, XYZ position, BoundingBoxXYZ boundingBox = null)
        {
            if (view != null)
            {
                Viewport viewport = Viewport.Create(Doc, _viewSheet.Id, view.Id, position);
                if (boundingBox != null)
                {
                    ApplyCropRegion(view, boundingBox);
                }
                // TransformCropRegion(view);
                return viewport;
            }
            return null;
        }

        private void ApplyCropRegion(View view, BoundingBoxXYZ boundingBox)
        {
            if (boundingBox == null) return;

            view.CropBox = boundingBox;
            view.CropBoxActive = true;
            view.CropBoxVisible = false;
        }


        private void TransformCropRegion(View view)
        {
            ElementId cropBoxElementId = ElementUtils.GetCropBoxOfView(view);

            BoundingBoxXYZ cropBox = view.CropBox;
            XYZ center = 0.5 * (cropBox.Min + cropBox.Max);
            Line axis = Line.CreateBound(center, center + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(Doc, cropBoxElementId, axis, _rotationAngle);

            BoundingBoxXYZ boundingBox = view.CropBox;

            XYZ expandedMin = new
            (
                boundingBox.Min.X - Math.Sign(boundingBox.Min.X) * _offset,
                boundingBox.Min.Y + Math.Sign(boundingBox.Min.Y) * _offset,
                boundingBox.Min.Z
            );

            XYZ expandedMax = new
            (
                boundingBox.Max.X + Math.Sign(boundingBox.Max.X) * _offset,
                boundingBox.Max.Y - Math.Sign(boundingBox.Max.Y) * _offset,
                boundingBox.Max.Z
            );


            view.CropBox = new BoundingBoxXYZ
            {
                Min = expandedMin,
                Max = expandedMax
            };

        }

        private ElementId GetViewTypeId(Document doc, string familyName, string typeName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(v => v.FamilyName == familyName &&
                                     v.FindParameter(BuiltInParameter.ALL_MODEL_TYPE_NAME)
                                      .AsValueString() == typeName)
                ?.Id ?? ElementId.InvalidElementId;
        }

        private void RotateElement(Element element, XYZ rotationCenter, double angle)
        {
            element.Rotate(CreateAxisByPoint(rotationCenter), angle);
        }

        private Line CreateAxisByPoint(XYZ xyz)
        {
            return Line.CreateUnbound(xyz, XYZ.BasisZ);
        }


        private ViewPlan CreateViewPlan(ViewFamily viewFamily)
        {
            var viewFamilyType = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == viewFamily);

            if (viewFamilyType == null || Doc.ActiveView.GenLevel == null)
                return null;

            return ViewPlan.Create(Doc, viewFamilyType.Id, Doc.ActiveView.GenLevel.Id);
        }
    }
}
