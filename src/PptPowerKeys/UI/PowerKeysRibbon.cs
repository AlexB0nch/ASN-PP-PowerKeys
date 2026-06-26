using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Office.Core;

namespace PptPowerKeys.UI
{
    public class PowerKeysRibbon : IRibbonExtensibility
    {
        private IRibbonUI _ribbon;

        public string GetCustomUI(string ribbonId)
        {
            return GetResourceText("PptPowerKeys.UI.RibbonTab.xml");
        }

        public void OnLoad(IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;
        }

        public void OnAction(IRibbonControl control)
        {
            Debug.WriteLine($"PowerKeys ribbon action '{control.Id}' — not implemented yet (S01-003+).");
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
