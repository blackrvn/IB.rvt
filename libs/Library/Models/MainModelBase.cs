using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Library.Interfaces;

namespace Library.Models
{
    public abstract class MainModelBase<T1, TModel> : ModelBase, IMainModel<T1, TModel>
    {
        protected readonly Func<T1, TModel> _modelFactory;
        public IList<T1> SelectedItems { get; set; }
        
        public MainModelBase(Func<T1, TModel> modelFactory)
        {
            _modelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
            SelectedItems = new List<T1>();
        }

        public virtual void ProcessItems()
        {
        }

        public virtual void CollectItems()
        {
            // Expand in child class to add to SelectedItems<T1>
        }

        public IList<Reference> GetUserSelection(ISelectionFilter selectionFilter)
        {
            return UIDocument.Selection.PickObjects(ObjectType.Element, selectionFilter);
        }
    }
}
