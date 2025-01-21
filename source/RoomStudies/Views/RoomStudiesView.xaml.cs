using RoomStudies.ViewModels;

namespace RoomStudies.Views
{
    public sealed partial class RoomStudiesView
    {
        public RoomStudiesView(MainViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}