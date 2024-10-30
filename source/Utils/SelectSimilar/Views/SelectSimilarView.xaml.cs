using SelectSimilar.ViewModels;

namespace SelectSimilar.Views
{
    public sealed partial class SelectSimilarView
    {
        public SelectSimilarView(SelectSimilarViewModel viewModel)
        {
            DataContext = viewModel;
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