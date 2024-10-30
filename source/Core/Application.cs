using Autodesk.Revit.UI;
using Core.Commands;
using Nice3point.Revit.Toolkit.External;
using Localization;
using Autodesk.Revit.ApplicationServices;
using System.Globalization;
using SelectSimilar.Views.Converters;
using Newtonsoft.Json.Linq;
using System.Net.Http;

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
            // await CheckForUpdates();

            TabName = "IB";
            Application.CreateRibbonTab(TabName);
            CreateRibbon();
        }

        private void CreateRibbon()
        {
            LanguageType languageType  = Application.ControlledApplication.Language;
            AutodeskLanguageConverter converter = new AutodeskLanguageConverter();
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

        public async Task CheckForUpdates()
        {
            string currentVersion = "1.0.0"; // Deine aktuelle Plugin-Version
            string url = "https://api.github.com/repos/<blackrvn>/<IB.rvt>/releases/latest";

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Dein-Plugin-Name");

                var response = await client.GetStringAsync(url);
                JObject release = JObject.Parse(response);

                string latestVersion = release["tag_name"].ToString();

                if (currentVersion != latestVersion)
                {
                    // Benutzer benachrichtigen
                    System.Windows.MessageBox.Show($"Eine neue Version {latestVersion} ist verfügbar! Besuche GitHub, um sie herunterzuladen.");
                }
            }
        }
    }
}