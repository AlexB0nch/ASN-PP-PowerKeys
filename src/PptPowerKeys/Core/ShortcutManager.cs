using System.Diagnostics;
using Microsoft.Office.Interop.PowerPoint;
using PptPowerKeys.Settings;

namespace PptPowerKeys.Core
{
    /// <summary>
    /// Registers and handles keyboard shortcuts. Full key interception — S01-002.
    /// </summary>
    public class ShortcutManager : IShortcutManager
    {
        private readonly Application _application;
        private readonly ICommandDispatcher _dispatcher;
        private UserSettings _settings;

        public ShortcutManager(Application application, ICommandDispatcher dispatcher)
        {
            _application = application;
            _dispatcher = dispatcher;
        }

        public void Initialize()
        {
            _settings = UserSettings.LoadDefaults();
            Debug.WriteLine($"ShortcutManager initialized with {_settings.Shortcuts.Count} default bindings.");
        }

        public void ReloadBindings()
        {
            _settings = UserSettings.Load();
            Debug.WriteLine("Shortcut bindings reloaded.");
        }

        public void Dispose()
        {
            _settings = null;
        }
    }
}
