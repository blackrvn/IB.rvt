using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Library.Models;
using Library.Utils;
using System.Diagnostics;
using System.Text;
using RoomStudies.Services;

namespace RoomStudies.Models
{

    public class RSModel : ModelBase
    {
        private readonly Room _room;
        private Autodesk.Revit.DB.Point _rotationCenter;
        private double _rotationAngle;
        private ViewSheet _viewSheet;
        private readonly double _offset;
        private bool _isRectangular = true;
        private List<Curve> _relevantSegments = new();

        public RSModel(Room room)
        {
            _room = room ?? throw new System.ArgumentNullException(nameof(room));
            _offset = UnitUtils.ConvertToInternalUnits(0.5, UnitTypeId.Meters);
        }

        public string RoomNumber => _room.Number;
        public string RoomName => _room.Name;

        public string SheetNumber => _viewSheet?.SheetNumber ?? "N/A";
        public string SheetName => _viewSheet?.Name ?? "N/A";


        private ElementId GetTitleBlockId()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol));
            collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
            collector.WhereElementIsElementType();
            return collector.FirstOrDefault().Id;
        }

        private void CreateSheet()
        {
            _viewSheet = ViewSheet.Create(Doc, GetTitleBlockId());
            if (_viewSheet == null) throw new System.InvalidOperationException("Sheet creation failed.");
        }

        public void CreateRoomStudy()
        {
            using var transactionManager = new TransactionHelper(Doc, $"Processing room: {_room.Number} - {_room.Name}");

            try
            {
                var settings = RSSettingsManager.Settings;
                transactionManager.Execute(new List<Action>([() => CreateSheet(), () => ExtractGeometricalData(), () => CreatePlanViews(), () => SetSheetAttributes()]));

                if (_isRectangular)
                {
                    ProcessRectangularRoom(transactionManager);
                }
                else
                {
                    ProcessNonRectangularRoom(transactionManager);
                }
            }

            catch (System.Exception e)
            {
                transactionManager.GetTransactionGroup().RollBack();
                throw new System.Exception(e.Message);
            }

        }

        private void ProcessRectangularRoom(TransactionHelper transactionHelper)
        {
            Element elevationMarker = null;
            transactionHelper.Execute(() => elevationMarker = CreateElevationMarker(_rotationCenter.Coord, "Elevation", "Interior Elevation"));
            if (elevationMarker is ElevationMarker marker)
            {
                View view = null;
                transactionHelper.Execute(() => view = marker.CreateElevation(Doc, Doc.ActiveView.Id, 0));
                transactionHelper.Execute(new List<Action>([
                    () => RotateElement(elevationMarker, _rotationCenter.Coord, _rotationAngle), 
                    () => TransformCropRegion(view, view.CropBox)]));
                CreateElevationViews(marker, transactionHelper);
            }
        }

        private void ProcessNonRectangularRoom(TransactionHelper transactionHelper)
        {
            transactionHelper.Execute(() =>
            {
                List<ViewSection> viewSections = CreateSectionView();
                for (int i = 0; i < viewSections.Count; i++)
                {
                    CreateViewPort(viewSections[i], new XYZ(i, i, 0));
                }
            });
        }

        private void SetSheetAttributes()
        {
            if (_viewSheet == null) throw new System.InvalidOperationException("Sheet has not been created.");

            try
            {
                _viewSheet.LookupParameter("Sheet Name")?.Set(_room.Name);
                _viewSheet.LookupParameter("Sheet Number")?.Set(_room.Number);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting sheet attributes: {e.Message}");
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

        private bool IsLine(Curve segment)
        {
            return segment is Line;
        }

        private bool IsCollinear(Curve segA, Curve segB, double tolerance = 1e-9)
        {
            var curveA = segA as Line;
            var curveB = segB as Line;
            if (curveA == null || curveB == null) return false;

            XYZ endA = curveA.GetEndPoint(1);
            XYZ startB = curveB.GetEndPoint(0);

            if (!endA.IsAlmostEqualTo(startB, tolerance))
            {
                return false;
            }


            XYZ vA = (curveA.GetEndPoint(1) - curveA.GetEndPoint(0)).Normalize();
            XYZ vB = (curveB.GetEndPoint(1) - curveB.GetEndPoint(0)).Normalize();

            double dot = vA.DotProduct(vB);
            return (Math.Abs(Math.Abs(dot) - 1.0) < tolerance);
        }

        private Curve MergeSegments(Curve segA, Curve segB)
        {
            var curveA = segA as Line;
            var curveB = segB as Line;

            XYZ start = curveA.GetEndPoint(0);
            XYZ end = curveB.GetEndPoint(1);

            Line mergedLine = Line.CreateBound(start, end);

            return (Curve)mergedLine;
        }


        private List<Curve> MergeCollinearSegments(IList<BoundarySegment> segmentList, double tolerance = 1e-9)
        {
            var result = new List<Curve>();
            if (segmentList.Count == 0) return result;


            int i = 0;
            while (i < segmentList.Count)
            {
                var currentSegment = segmentList[i].GetCurve();

                if (!IsLine(currentSegment))
                {
                    result.Add(currentSegment);
                    i++;
                    continue;
                }

                int j = i + 1;
                while (j < segmentList.Count)
                {
                    var nextSegment = segmentList[j].GetCurve();
                    if (IsLine(nextSegment) && IsCollinear(currentSegment, nextSegment, tolerance))
                    {
                        currentSegment = MergeSegments(currentSegment, nextSegment);
                        j++;
                    }
                    else
                    {
                        break;
                    }
                }

                result.Add(currentSegment);

                i = j;
            }

            return result;
        }



        private (Autodesk.Revit.DB.Point, XYZ) CalculateCentroid(IList<IList<BoundarySegment>> segments)
        {
            double x = 0, y = 0, z = 0, maxLength = 0;
            int numSegments = 0;
            XYZ referenceVector = XYZ.Zero;
            double threshold = UnitUtils.ConvertToInternalUnits(0.5, UnitTypeId.Meters);

            foreach (var segmentList in segments)
            {
                var mergedSegments = MergeCollinearSegments(segmentList);

                for (int i = 0; i < mergedSegments.Count; i++)
                {
                    Curve currentBoundaryCurve = mergedSegments[i];
                    numSegments++;
                    XYZ start = currentBoundaryCurve.GetEndPoint(0);
                    XYZ end = currentBoundaryCurve.GetEndPoint(1);

                    x += start.X;
                    y += start.Y;
                    z += start.Z;

                    if (currentBoundaryCurve.ApproximateLength > maxLength)
                    {
                        maxLength = currentBoundaryCurve.ApproximateLength;
                        referenceVector = new XYZ(end.X - start.X, end.Y - start.Y, 0);
                        CreateDebugCurve(Line.CreateBound(end, start));
                    }


                    if (i+1 <= segmentList.Count - 1)
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

                    if (currentBoundaryCurve.ApproximateLength >= threshold)
                    {
                        _relevantSegments.Add(currentBoundaryCurve);
                    }
                }
            }

            XYZ centroid = new(x / numSegments, y / numSegments, z / numSegments);
            return (Autodesk.Revit.DB.Point.Create(centroid), referenceVector);
        }

        
        private double CalculateAngle(XYZ referenceVector)
        {
            XYZ unitVector = XYZ.BasisX;
            double angle =  referenceVector.X < 0
                ? referenceVector.Negate().Normalize().AngleTo(unitVector)
                : referenceVector.Normalize().AngleTo(unitVector);
            return Math.Sign(referenceVector.X) * angle;
        }
        

        /*
        private double CalculateAngle(XYZ referenceVector)
        {
            double angle = Math.Atan2(referenceVector.X, referenceVector.Y);
            return Math.PI / 2 + Math.Sign(referenceVector.X) * angle;
        }
        */

        public ISet<ElementId> GetViewSet() => _viewSheet?.GetAllPlacedViews() ?? new HashSet<ElementId>();

        public IList<ElementId> GetAllElementsOnSheet()
        {
            if (_viewSheet == null) return new List<ElementId>();

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

        private BoundingBoxXYZ CreateSectionBoundingBox(Curve segment)
        {
            Level level = _room.Level;
            double baseElevation = level.Elevation;
            Level upperLevel = Doc.GetElement(_room.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL).AsElementId()) as Level;
            double upperLevelElevation = upperLevel.Elevation;
            double upperOffset = _room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).AsDouble();
            double topElevation = upperLevelElevation + upperOffset;

            XYZ p0 = segment.GetEndPoint(0);
            XYZ p1 = segment.GetEndPoint(1);

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
            Debug.WriteLine($"\nX: {t.BasisX}, Y: {t.BasisY}, Z: {t.BasisZ}\n");

            double roomHeight = topElevation - baseElevation + 2 *_offset;
            double segmentLength = segment.ApproximateLength;

            BoundingBoxXYZ boundingBox = new()
            {
                Transform = t,

                Min = new XYZ(-_offset, -_offset, UnitUtils.ConvertToInternalUnits(-1, UnitTypeId.Meters)),
                Max = new XYZ(segmentLength + _offset, roomHeight, UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Meters)),

                Enabled = true
            };

            return boundingBox;

        }

        private List<ViewSection> CreateSectionView()
        {
            List<ViewSection> sectionViews = new();
            if (_relevantSegments.Count == 0)
            {
                TaskDialog.Show("Error", "No relevant segments found.");
                return null;
            }
            else
            {
                for (int i = 0; i<_relevantSegments.Count; i++)
                {
                    Curve curve = _relevantSegments[i];
                    ElementId viewFamilyTypeId = GetViewTypeId(Doc, "Section", "Building Section");
                    BoundingBoxXYZ sectionBoundingBox = CreateSectionBoundingBox(curve);
                    Debug.WriteLine($"Min: {sectionBoundingBox.Min}\nMax: {sectionBoundingBox.Max}");
                    try
                    {
                        ViewSection viewSection = ViewSection.CreateSection(Doc, viewFamilyTypeId, sectionBoundingBox);
                        SetSectionAttributes(viewSection, i);
                        sectionViews.Add(viewSection);
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException AE)
                    {
                        throw new System.ArgumentException(AE.Message);
                    }

                }
                return sectionViews;
            }
        }

        private void CreateElevationViews(ElevationMarker elevationMarker, TransactionHelper transactionHelper)
        {
            Debug.WriteLine($"------ {_room.Number} - {_room.Name} ------");
            for (int i = 1; i < 4; i++)
            {
                transactionHelper.Execute(() =>
                {
                    bool isOccupied = !elevationMarker.IsAvailableIndex(i);
                    if (isOccupied)
                    {
                        ElementId viewId = elevationMarker.GetViewId(i);
                        Doc.Delete(viewId);
                        Debug.WriteLine($"\n------ View at Index {i} was deleted ------\n");
                    }
                });

                View newView = null;

                transactionHelper.Execute(() =>
                {
                    Debug.WriteLine($"------ Section: {i + 1} ------\n");
                    newView = elevationMarker.CreateElevation(Doc, Doc.ActiveView.Id, i);
                    Debug.WriteLine($"Length untransformed: {(newView.CropBox.Max - newView.CropBox.Min).GetLength()}");
                });

                transactionHelper.Execute(() =>
                {
                    TransformCropRegion(newView, newView.CropBox);
                    CreateViewPort(newView, new XYZ(i, i, 0));
                });
            }
        }

        private Viewport CreateViewPort(View view, XYZ position)
        {
            if (view != null)
            {
                Viewport viewport = Viewport.Create(Doc, _viewSheet.Id, view.Id, position);
                SetBoundingBoxAttributes(view);
                return viewport;
            }
            return null;
        }

        private void SetBoundingBoxAttributes(View view)
        {
            if (view.CropBox == null) return;

            view.CropBoxActive = true;
            view.CropBoxVisible = true;
        }


        private void TransformCropRegion(View view, BoundingBoxXYZ boundingBox)
        {
            Transform transform = boundingBox.Transform;
            Debug.WriteLine($"Bases: \nX: {transform.BasisX}, Y: {transform.BasisY}, Z: {transform.BasisZ}\n");
            Debug.WriteLine($"Determinant: {transform.Determinant}");

            XYZ offsetVector = transform.BasisX * _offset + transform.BasisY * _offset + transform.BasisZ * _offset;

            XYZ minTransformed = transform.OfPoint(boundingBox.Min); //Model Coordinate
            XYZ maxTransformed = transform.OfPoint(boundingBox.Max); //Model Coordinate

            Debug.WriteLine($"BoundingBox Transform:\n{transform}");
            Debug.WriteLine($"Min (local): {boundingBox.Min}, Max (local): {boundingBox.Max}");
            Debug.WriteLine($"Min (world): {minTransformed}, Max (world): {maxTransformed}");

            XYZ expandedMin = minTransformed - offsetVector;
            XYZ expandedMax = maxTransformed + offsetVector;

            Debug.WriteLine($"Expanded Min (world): {expandedMin}, Expanded Max (world): {expandedMax}");

            XYZ finalMin = transform.Inverse.OfPoint(expandedMin);
            XYZ finalMax = transform.Inverse.OfPoint(expandedMax);

            XYZ correctedMin = new XYZ(
                Math.Min(finalMin.X, finalMax.X),
                Math.Min(finalMin.Y, finalMax.Y),
                Math.Min(finalMin.Z, finalMax.Z)
            );

            XYZ correctedMax = new XYZ(
                Math.Max(finalMin.X, finalMax.X),
                Math.Max(finalMin.Y, finalMax.Y),
                Math.Max(finalMin.Z, finalMax.Z)
            );

            BoundingBoxXYZ newBoundingBox = new BoundingBoxXYZ
            {
                Min = correctedMin, // Korrigierte Werte setzen
                Max = correctedMax,
                Transform = transform // Transform beibehalten
            };

            Debug.WriteLine($"Final Min (local): {newBoundingBox.Min}, Final Max (local): {newBoundingBox.Max}");
            Debug.WriteLine($"Length Old: {UnitUtils.ConvertFromInternalUnits((maxTransformed - minTransformed).GetLength(), UnitTypeId.Meters)}\nLength new: {UnitUtils.ConvertFromInternalUnits((correctedMax - correctedMin).GetLength(), UnitTypeId.Meters)}");

            view.CropBox = newBoundingBox;


            if (view.ViewType == ViewType.CeilingPlan || view.ViewType == ViewType.FloorPlan)
            {
                ElementId cropBoxElementId = ElementUtils.GetCropBoxOfView(view);
                Line axis = Line.CreateBound(_rotationCenter.Coord, _rotationCenter.Coord + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(Doc, cropBoxElementId, axis, _rotationAngle);
            }
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
            ElementTransformUtils.RotateElement(Doc, element.Id, CreateAxisByPoint(rotationCenter), angle);
        }


        private Line CreateAxisByPoint(XYZ xyz)
        {
            return Line.CreateUnbound(xyz, XYZ.BasisZ);
        }

        private void CreatePlanViews()
        {
            ViewPlan Create(ViewFamily viewFamily)
            {
                var viewFamilyType = new FilteredElementCollector(Doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft => vft.ViewFamily == viewFamily);

                if (viewFamilyType == null || Doc.ActiveView.GenLevel == null)
                    return null;

                return ViewPlan.Create(Doc, viewFamilyType.Id, Doc.ActiveView.GenLevel.Id);
            }

            ViewPlan floorPlan = Create(ViewFamily.FloorPlan);
            ViewPlan ceilingPlan = Create(ViewFamily.CeilingPlan);

            TransformCropRegion(floorPlan, _room.get_BoundingBox(floorPlan));
            TransformCropRegion(ceilingPlan, _room.get_BoundingBox(ceilingPlan));

            CreateViewPort(floorPlan, XYZ.Zero);
            CreateViewPort(ceilingPlan, XYZ.Zero);

        }



        private void CreateDebugCurve(Curve curve)
        {
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, curve.GetEndPoint(0));
            SketchPlane sketchPlane = SketchPlane.Create(Doc, plane);

            ModelCurve modelCurve = Doc.Create.NewModelCurve(curve, sketchPlane);
            Debug.WriteLine($"Start: {modelCurve.GeometryCurve.GetEndPoint(0)}\nEnd: {modelCurve.GeometryCurve.GetEndPoint(1)}");

        }

        private void LoadSettings()
        { 
        }
    }
}
