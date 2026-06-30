using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Office.Core;
using PptPowerKeys.Core.Commands;

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

        public bool GetSnapToGridPressed(IRibbonControl control)
        {
            return Globals.ThisAddIn?.SettingsStore?.Current.SnapToGrid ?? false;
        }

        public void OnSnapToGridToggle(IRibbonControl control, bool pressed)
        {
            var store = Globals.ThisAddIn?.SettingsStore;
            if (store == null)
            {
                MessageBox.Show(
                    "Settings store is not initialized.",
                    "PPT PowerKeys",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            store.SetSnapToGrid(pressed);
            _ribbon?.InvalidateControl(control.Id);
            Debug.WriteLine($"SnapToGrid set to {pressed} (persisted to UserSettings.json).");
        }

        public void OnLayoutCommand(IRibbonControl control)
        {
            if (!RibbonCommandMap.TryParse(control.Id, out var command))
            {
                MessageBox.Show(
                    $"Unknown layout command for ribbon control '{control.Id}'.",
                    "PPT PowerKeys",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ExecuteLayout(command);
        }

        private static void ExecuteLayout(CommandIds command)
        {
            var router = Globals.ThisAddIn?.CommandRouter;
            if (router == null)
            {
                MessageBox.Show(
                    "Command router is not initialized.",
                    "PPT PowerKeys",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var title = CommandCatalog.Find(command)?.Title ?? command.ToString();

            try
            {
                var result = router.Execute(command);
                Debug.WriteLine(
                    result.Changed
                        ? $"{command} applied via Core.LayoutEngine (in-process)."
                        : $"{command} no-op: {result.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"{command} failed: {ex}");
                MessageBox.Show(ex.Message, $"PPT PowerKeys — {title}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
