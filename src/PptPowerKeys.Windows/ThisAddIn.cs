namespace PptPowerKeys.Windows
{
    public partial class ThisAddIn
    {
        private Settings.WindowsUserSettingsStore _settingsStore;
        private Host.CommandRouter _commandRouter;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            _settingsStore = new Settings.WindowsUserSettingsStore();
            var host = new Host.ComHostAdapter(Application);
            _commandRouter = new Host.CommandRouter(host, _settingsStore);
            System.Diagnostics.Debug.WriteLine(
                $"PptPowerKeys.Windows loaded. Core catalog: {Core.Commands.CommandCatalog.All.Count} commands. " +
                $"SnapToGrid={_settingsStore.Current.SnapToGrid}.");
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            _commandRouter = null;
            _settingsStore = null;
        }

        internal Host.CommandRouter CommandRouter => _commandRouter;

        internal Settings.WindowsUserSettingsStore SettingsStore => _settingsStore;

        #region VSTO generated code

        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
