namespace PptPowerKeys.Windows
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            // S07-002 bootstrap: no host wiring yet (CommandRouter in S07-003).
            System.Diagnostics.Debug.WriteLine(
                $"PptPowerKeys.Windows loaded. Core catalog: {Core.Commands.CommandCatalog.All.Count} commands.");
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
