namespace PptPowerKeys.Windows
{
    public partial class ThisAddIn
    {
        private Host.CommandRouter _commandRouter;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            var host = new Host.ComHostAdapter(Application);
            _commandRouter = new Host.CommandRouter(host);
            System.Diagnostics.Debug.WriteLine(
                $"PptPowerKeys.Windows loaded. Core catalog: {Core.Commands.CommandCatalog.All.Count} commands.");
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            _commandRouter = null;
        }

        internal Host.CommandRouter CommandRouter => _commandRouter;

        #region VSTO generated code

        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
