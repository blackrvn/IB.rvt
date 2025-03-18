using RoomStudies.Models;
using Library.Services;

namespace RoomStudies.Services
{
    class RSSettingsManager : ModuleSettingsProvider<RSSettingsModel>
    {
        public static RSSettingsModel Settings => GetSettings(model => model.LoadSettings());
        public static void ReloadSettings() => ReloadSettings(model => model.LoadSettings());
    }
}
