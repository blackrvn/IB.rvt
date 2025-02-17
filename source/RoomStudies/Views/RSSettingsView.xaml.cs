using Library.Views;
using RoomStudies.ViewModels;

namespace RoomStudies.Views
{
    public sealed partial class RSSettingsView : BaseView
    {
        public RSSettingsView(RSSettingsViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}