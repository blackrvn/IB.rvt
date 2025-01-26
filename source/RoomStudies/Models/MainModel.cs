using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using Library.Interfaces;
using Library.Models;
using Library.Filters;

namespace RoomStudies.Models
{
    public class MainModel : MainModelBase<Room, RoomStudyModel>
    {
        public MainModel(Func<Room, RoomStudyModel> modelFactory) : base(modelFactory)
        {
            CollectItems();
            ProcessItems();
        }

        public override void ProcessItems() 
        {
            ElementId titleBlockId = GetTitleBlockId();
            foreach (var item in SelectedItems)
            {
                var model = _modelFactory(item);
                model.CreateRoomStudy(titleBlockId);
            }

        }
        public override void CollectItems()
        {
            try
            {
                ISelectionFilter selectionFilter = new BuiltInCategorySelectionFilter(BuiltInCategory.OST_Rooms);

                IList<Reference> references = UIDocument.Selection.PickObjects(ObjectType.Element, selectionFilter);
                foreach (Reference reference in references)
                {
                    SelectedItems.Add(reference.ElementId.ToElement(Doc) as Room);
                }
            }
            catch (OperationCanceledException OCE)
            {
                Console.WriteLine(OCE);
                ModelTransaction.RollBack();
                throw new OperationCanceledException("Selection was cancelled");
            }

        }

        private ElementId GetTitleBlockId()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol));
            collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
            collector.WhereElementIsElementType();
            return collector.FirstOrDefault().Id;
        }
    }
}
