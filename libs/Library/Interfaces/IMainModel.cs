using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Interfaces
{
    public interface IMainModel<T1, TModel>
    {
        IList<T1> SelectedItems { get; set; }
        void CollectItems();
        void ProcessItems();
        IList<Reference> GetUserSelection(ISelectionFilter selectionFilter);
    }


}
