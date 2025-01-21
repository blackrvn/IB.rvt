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
            ElementId titleBlockId = GetTitleBlockId("E1 30x42 Horizontal");
            var transaction = new Transaction(Doc);
            transaction.Start("Processing selected rooms");
            foreach (var item in SelectedItems)
            {
                var model = _modelFactory(item);
                model.CreateRoomStudy(titleBlockId);
            }
            transaction.Commit();
        }
        public override void CollectItems() // Collects rooms from user selection
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
            }

        }

        private ElementId GetTitleBlockId(string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol));
            collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
            collector.WhereElementIsElementType();
            foreach (Element familySymbol in collector)
            {
                if (familySymbol.Name == name)
                {
                    return familySymbol.Id;
                }
            }
            return null;
        }
    }
}
