using Autodesk.Revit.ApplicationServices;
using Core.ViewModels;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Core.Views
{
    /// <summary>
    /// Interaction logic for UpdateNotification.xaml
    /// </summary>
    public partial class UpdateNotification : Window
    {

        public UpdateNotification(CoreViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.LoadAssembly();
            InitializeComponent();
            if (viewModel != null)
            {
                viewModel.RequestClose += OnRequestClose;
            }
        }

        private void OnRequestClose()
        {
            this.Close(); // Schließe das Fenster
        }
    }
}
