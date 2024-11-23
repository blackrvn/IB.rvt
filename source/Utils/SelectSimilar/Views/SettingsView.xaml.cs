using SelectSimilar.ViewModels;
using Library.Views;

namespace SelectSimilar.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : BaseView
    {
        public SettingsView(SettingsViewModel settingsViewModel) : base(settingsViewModel)
        {
            InitializeComponent();
        }
    }
}