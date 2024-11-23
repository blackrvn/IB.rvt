using Core.ViewModels;
using Core.Views;
using static Constants;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Creation;



namespace Core.Management
{
    internal class UpdateManager
    {
        internal ControlledApplication ControlledApplication { get; set; }
        internal GitHubHelper GitHubHelper { get; set; }
        public async Task CheckForUpdates(ControlledApplication controlledApplication)
        {
            GitHubHelper = new();

            ControlledApplication = controlledApplication;

            string latestRelease = await GitHubHelper.GetLatestReleaseAsync("blackrvn", "IB.rvt");

            if (latestRelease != PlugInVersion)
            {
                //ShowUpdateNotification(latestRelease);
                SendUpdateNotificationToTeams(latestRelease);

            }
        }

        private void ShowUpdateNotification(string releaseVersion)
        
        {
            CoreViewModel viewModel = new(releaseVersion, GitHubHelper, ControlledApplication);
            UpdateNotification updateNotification = new(viewModel);
            updateNotification.ShowDialog();
        }

        private void SendUpdateNotificationToTeams(string releaseVersion)
        {

        }
    }
}
