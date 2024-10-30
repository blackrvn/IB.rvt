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
    public class SUCSelectSimilarSettings : ExternalCommand
    {
        public override void Execute()
        {
            Model model = new(UiDocument, true);
            SettingsViewModel settingsViewModel  = new(model);
            SettingsView view = new(settingsViewModel);
            view.ShowDialog();

        }
    }
}