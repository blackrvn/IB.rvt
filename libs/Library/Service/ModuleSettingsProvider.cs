
namespace Library.Services
{
    public abstract class ModuleSettingsProvider<TModel> where TModel : class, new()
    {
        private static TModel _settings;
        private static readonly object _lock = new object();

        protected static TModel GetSettings(Action<TModel> initializeAction = null)
        {
            if (_settings == null)
            {
                lock (_lock)
                {
                    if (_settings == null)
                    {
                        _settings = new TModel();
                        initializeAction?.Invoke(_settings);
                    }
                }
            }
            return _settings;
        }

        protected static void ReloadSettings(Action<TModel> reloadAction)
        {
            lock (_lock)
            {
                if (_settings != null)
                {
                    reloadAction(_settings);
                }
            }
        }

    }
}
