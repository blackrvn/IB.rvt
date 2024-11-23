using Library.ViewModels;
using System.Windows;

namespace Library.Views
{
    public class BaseView : Window
    {
        public BaseView(BaseViewModel viewModel)
        {
            DataContext = viewModel;
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