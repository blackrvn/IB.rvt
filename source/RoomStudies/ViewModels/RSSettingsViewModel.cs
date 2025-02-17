using Library.ViewModels;
using RoomStudies.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace RoomStudies.ViewModels
{
    public sealed partial class RSSettingsViewModel : BaseViewModel
    {
        private readonly RSSettingsModel _model;

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private List<object> _views = new List<object>();



        public RSSettingsViewModel(RSSettingsModel model)
        {
            _model = model;
            CurrentView = new RSTypeMenuViewModel(_model);
            InitializeUIStrings("Settings", "OK", "Cancel");
            InitializeViews();
            InitializeCommands();
        }

        private void InitializeViews()
        {
            Views.Add(new RSTypeMenuViewModel(_model));
            Views.Add(new RSNamingMenuViewModel(_model));
        }



        public override void InitializeCommands()
        {
            base.InitializeCommands();

            PropertyChanged += CurrentView_PropertyChanged;

        }

        private void CurrentView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentView))
            {
                Debug.WriteLine("Menu");
            }
        }
    }
}