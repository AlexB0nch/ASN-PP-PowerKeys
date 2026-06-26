namespace PptPowerKeys
{
    public partial class ThisAddIn
    {
        private Core.IShortcutManager _shortcutManager;
        private Core.ICommandDispatcher _commandDispatcher;
        private Core.IColorSchemeReader _colorSchemeReader;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            _colorSchemeReader = new Core.ColorSchemeReader(Application);
            _commandDispatcher = new Core.CommandDispatcher(Application);
            _shortcutManager = new Core.ShortcutManager(Application, _commandDispatcher);
            _shortcutManager.Initialize();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            _shortcutManager?.Dispose();
            _shortcutManager = null;
            _commandDispatcher = null;
            _colorSchemeReader = null;
        }

        internal Core.ICommandDispatcher CommandDispatcher => _commandDispatcher;

        #region VSTO generated code

        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
