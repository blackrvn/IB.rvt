using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Library.Interfaces;

namespace Library.Models
{
    public abstract class MainModelBase<T, TModel> : ModelBase, IMainModel<T, TModel>
    {
        protected readonly Func<T, TModel> _modelFactory;
        public IList<T> SelectedItems { get; set; }
        
        public MainModelBase(Func<T, TModel> modelFactory)
        {
            _modelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
            SelectedItems = new List<T>();
        }

        public virtual void ProcessItems()
        {
            foreach (var item in SelectedItems)
            {
                var model = _modelFactory(item);
            }
        }

        public virtual void CollectItems()
        {
            // Expand in child class to add to SelectedItems<T>
        }

        public IList<Reference> GetUserSelection(ISelectionFilter selectionFilter)
        {
            return UIDocument.Selection.PickObjects(ObjectType.Element, selectionFilter);
        }
    }
}
