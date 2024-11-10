using Autodesk.Revit.UI;
using Core.Commands;
using Nice3point.Revit.Toolkit.External;
using Localization;
using Autodesk.Revit.ApplicationServices;
using System.Globalization;
using SelectSimilar.Views.Converters;
using Core.Management;
using Autodesk.Revit.Creation;

namespace Core
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public RibbonPanel RibbonPanel { get; private set; }
        public string TabName { get; private set; }

        public override async void OnStartup()
        {
            UpdateManager updateManager = new();
            await updateManager.CheckForUpdates(Application.ControlledApplication);

            TabName = "IB+";
            Application.CreateRibbonTab(TabName);
            CreateRibbon();
        }

        private void CreateRibbon()
        {
            LanguageType languageType  = Application.ControlledApplication.Language;
            AutodeskLanguageConverter converter = new();
            CultureInfo cultureInfo = converter.Convert(languageType);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            RibbonPanel = Application.CreatePanel(UIElements.Utils_PanelName, TabName);

            PulldownButton pullDownButton = (PulldownButton)RibbonPanel.AddPullDownButton("ibssPullDownButton", PushButtons.SelectSimilar_MainName)
                .SetImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageLarge.png");

            pullDownButton.AddPushButton<SUCSelectSimilar>(PushButtons.SelectSimilar_MainName)
                .SetImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/SelectSimilar_PushButtonImageLarge.png");

            pullDownButton.AddPushButton<SUCSelectSimilarSettings>(PushButtons.SelectSimilar_SettingsName)
                .SetImage("/Core;component/Resources/Icons/SelectSimilar_SettingsImageSmall.png")
                .SetLargeImage("/Core;component/Resources/Icons/SelectSimilar_SettingsImageLarge.png");

        }



    }
}