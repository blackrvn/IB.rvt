using Autodesk.Revit.UI;
using Core.Commands;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Extensions;
using Localization;
using Autodesk.Revit.ApplicationServices;
using System.Globalization;
using SelectSimilar.Views.Converters;
using Core.Management;
using Autodesk.Revit.Creation;
using System.Windows.Controls;
using Autodesk.Revit.DB;

namespace Core
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public RibbonPanel RibbonPanelUtils { get; private set; }
        public string TabName { get; private set; }

        public override void OnStartup()
        {
            // UpdateManager updateManager = new();
            // await updateManager.CheckForUpdates(Application.ControlledApplication);

            Host.Start();

            TabName = "IB⁺ Tools";
            Application.CreateRibbonTab(TabName);
            CreateRibbon();
        }

        public override void OnShutdown()
        {
            Host.Stop();
        }

        private void CreateRibbon()
        {
            LanguageType languageType  = Application.ControlledApplication.Language;
            AutodeskLanguageConverter converter = new();
            CultureInfo cultureInfo = converter.Convert(languageType);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            RibbonPanel RibbonPanelUtils = Application.CreatePanel(UIElements.Utils_PanelName, TabName);

            PulldownButton pullDownButton = (PulldownButton)RibbonPanelUtils.AddPullDownButton("ibssPullDownButton", PushButtons.SelectSimilar_MainName)
                .SetImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageLarge.png");

            pullDownButton.AddPushButton<SUCSelectSimilar>(PushButtons.SelectSimilar_MainName)
                .SetImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageLarge.png");

            pullDownButton.AddPushButton<SUCSelectSimilarSettings>(PushButtons.SelectSimilar_SettingsName)
                .SetImage("/Core;component/Resources/Icons/SelectSimilar_SettingsImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/SelectSimilar_SettingsImageLarge.png");


            

            RibbonPanel RibbonPanelRoomStudies = Application.CreatePanel("Roomstudies", TabName);

            PushButton pushButtonRS = (PushButton)RibbonPanelRoomStudies.AddPushButton<SUCRoomStudies>("RoomStudies")
                .SetImage("/Core;component/Resources/Icons/RoomStudies_PushButtonImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/RoomStudies_PushButtonImageLarge.png");



        }



    }
}