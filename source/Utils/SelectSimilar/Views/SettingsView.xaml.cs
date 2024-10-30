using Autodesk.Revit.DB;
using SelectSimilar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SelectSimilar.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView(SettingsViewModel settingsViewModel)
        {
            DataContext = settingsViewModel;
            InitializeComponent();
            if (settingsViewModel != null)
            {
                settingsViewModel.RequestClose += OnRequestClose;
            }
        }
        private void OnRequestClose()
        {
            this.Close(); // Schließe das Fenster
        }
    }
}
