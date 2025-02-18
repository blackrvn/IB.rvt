using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RoomStudies.Models;
using RoomStudies.ViewModels;
using RoomStudies.Views;
using Core;
using Autodesk.Revit.Exceptions;
using Core.Management;

namespace Core.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class SUCRoomStudies : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                RSMainModel mainModel = Host.GetService<RSMainModel>();
            }
            catch (System.OperationCanceledException e)
            {
                Result = Autodesk.Revit.UI.Result.Cancelled;
                ErrorMessage = e.Message;
            }
            catch (System.ArgumentException e)
            {
                Result = Autodesk.Revit.UI.Result.Failed;
                ErrorMessage = e.Message;
            }
            catch (Exception e)
            {
                Result = Autodesk.Revit.UI.Result.Failed;
                ErrorMessage = e.Message;
            }
        }
    }


    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class SUCRoomStudiesSettings : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                    RSSettingsView view = Host.GetService<RSSettingsView>();
                view.ShowDialog();
            }
            catch (Exception e)
            {
                Result = Autodesk.Revit.UI.Result.Failed;
                ErrorMessage = e.Message;
            }
        }
    }
}