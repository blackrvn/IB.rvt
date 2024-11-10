using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Core.Management;
using Localization;
using SelectSimilar.Models;

namespace Core.ViewModels
{
    public partial class CoreViewModel : ObservableObject
    {
        // UI Strings
        public string UpdateHeader { get; private set; }
        [ObservableProperty]
        public string updateMessage;
        public string UpdateNow { get; private set; }
        public string UpdateLater { get; private set; }

        // Commands
        public IRelayCommand UpdateNowCommand { get; private set; }
        public IRelayCommand CancelCommand { get; private set; }
        public event Action RequestClose;



        internal GitHubHelper GitHubHelper { get; private set; }

        public bool IsMultiUser { get; private set; }

        public ControlledApplication ControlledApplication { get; private set; }

        public CoreViewModel(string releaseVersion, GitHubHelper gitHubHelper, ControlledApplication controlledApplication)
        {
            UpdateHeader = UIElements.Update_UpdateHeader;
            UpdateMessage = string.Format(UIElements.Update_UpdateMessage, releaseVersion);
            UpdateNow = UIElements.Update_UpdateNow;
            UpdateLater = UIElements.Update_UpdateLater;
            GitHubHelper = gitHubHelper;
            ControlledApplication = controlledApplication;

            UpdateNowCommand = new RelayCommand(Update);
            CancelCommand = new RelayCommand(Cancel);

        }

        public void Update()
        {
            Debug.WriteLine("Update");
            GitHubHelper.DownloadLatesRelease(IsMultiUser);
            UpdateMessage = UIElements.Update_DownloadMessage;
        }

        public virtual void Cancel()
        {
            Debug.WriteLine("Cancel");
            RequestClose?.Invoke();
        }

        public void LoadAssembly()
        {
            string AllUsersLibPath = System.IO.Path.Combine(ControlledApplication.AllUsersAddinsLocation, "Core", "Library.dll");
            string CurrentUserLibPath = System.IO.Path.Combine(ControlledApplication.CurrentUserAddinsLocation, "Core", "Library.dll");

            if (File.Exists(AllUsersLibPath))
            {
                // Installation im Programmordner von Revit
                Assembly.LoadFrom(AllUsersLibPath);
                IsMultiUser = true;
            }
            else if (File.Exists(CurrentUserLibPath))
            {
                // Installation im User-Ordner von Revit
                Assembly.LoadFrom(CurrentUserLibPath);
                IsMultiUser = false;
            }
        }
    }
}
