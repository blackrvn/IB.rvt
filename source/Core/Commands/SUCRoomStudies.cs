using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RoomStudies.Models;
using RoomStudies.ViewModels;
using RoomStudies.Views;
using Core;
using Autodesk.Revit.Exceptions;

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
                MainModel mainModel = Host.GetService<MainModel>();
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
}