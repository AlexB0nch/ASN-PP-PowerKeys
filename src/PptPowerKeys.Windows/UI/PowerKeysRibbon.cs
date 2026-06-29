using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Office.Core;

namespace PptPowerKeys.Windows.UI
{
    public class PowerKeysRibbon : IRibbonExtensibility
    {
        private IRibbonUI _ribbon;

        public string GetCustomUI(string ribbonId)
        {
            return GetResourceText("PptPowerKeys.Windows.UI.RibbonTab.xml");
        }

        public void OnLoad(IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;
        }

        public void OnBootstrapAction(IRibbonControl control)
        {
            var commandCount = Core.Commands.CommandCatalog.All.Count;
            Debug.WriteLine($"PowerKeys.Windows bootstrap: {commandCount} commands in shared Core catalog.");
            MessageBox.Show(
                $"PptPowerKeys.Windows loaded.\nShared Core catalog: {commandCount} commands.",
                "PPT PowerKeys",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static string GetResourceText(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string fullName = assembly.GetName().Name + "." + resourceName.Replace('\\', '.').Replace('/', '.');

            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (string.Equals(name, fullName, System.StringComparison.OrdinalIgnoreCase))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(name))
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            throw new FileNotFoundException($"Embedded ribbon resource not found: {fullName}");
        }
    }
}
