using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Interfaces
{
    public interface IMainModel<T, TModel>
    {
        IList<T> SelectedItems { get; set; }
        void CollectItems();
        void ProcessItems();
        IList<Reference> GetUserSelection(ISelectionFilter selectionFilter);
    }


}
