

using RoomStudies.Models;

namespace RoomStudies.ViewModels
{
    partial class RSNamingMenuViewModel : ObservableObject
    {
        private readonly RSSettingsModel _model;
        public string Name { get; private set; }

        public RSNamingMenuViewModel(RSSettingsModel model)
        {
            _model = model;
            Name = "Naming";
        }
    }
}
