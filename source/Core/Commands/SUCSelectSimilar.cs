using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using SelectSimilar.Models;
using SelectSimilar.ViewModels;
using SelectSimilar.Views;

namespace Core.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class SUCSelectSimilar : ExternalCommand
    {
        public override void Execute()
        {
            Model model = new(UiDocument);
            SelectSimilarViewModel selectSimilarViewModel = new(model);
            if (selectSimilarViewModel.SuppressMainDialog)
            {
                model.Filter
                    (
                        selectSimilarViewModel.CheckedParameters,
                        selectSimilarViewModel.CategoryIsChecked,
                        selectSimilarViewModel.VisibleInViewIsChecked
                    );
            }
            else
            {
                SelectSimilarView view = new(selectSimilarViewModel);
                view.ShowDialog();
            }
        }
    }
}