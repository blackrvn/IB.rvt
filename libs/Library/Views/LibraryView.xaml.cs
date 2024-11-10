using Library.ViewModels;

namespace Library.Views
{
    public sealed partial class LibraryView
    {
        public LibraryView(LibraryViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}