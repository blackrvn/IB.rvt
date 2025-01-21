using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RoomStudies.Models;
using RoomStudies.ViewModels;
using RoomStudies.Views;
using Core;

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
            MainModel mainModel = Host.GetService<MainModel>();
        }
    }
}