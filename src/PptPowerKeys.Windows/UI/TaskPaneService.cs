using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Office.Core;
using Microsoft.Office.Tools;
using PptPowerKeys.Windows.Settings;

namespace PptPowerKeys.Windows.UI
{
    /// <summary>
    /// Hosts the WPF <see cref="SettingsPane"/> in a VSTO <see cref="CustomTaskPane"/>.
    /// </summary>
    public sealed class TaskPaneService : ITaskPaneService
    {
        private readonly CustomTaskPane _pane;
        private readonly SettingsPane _settingsPane;

        public TaskPaneService(
            CustomTaskPaneCollection panes,
            WindowsUserSettingsStore store,
            Action onSettingsSaved)
        {
            if (panes is null)
            {
                throw new ArgumentNullException(nameof(panes));
            }

            if (store is null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            _settingsPane = new SettingsPane(store, onSettingsSaved);
            var host = new ElementHost
            {
                Child = _settingsPane,
                Dock = DockStyle.Fill,
            };

            _pane = panes.Add(host, "PowerKeys Settings");
            _pane.Visible = false;
            _pane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
            _pane.Width = 320;
        }

        public void ShowSettings()
        {
            _settingsPane.SelectGeneralTab();
            _pane.Visible = true;
        }

        public void ShowSettingsScrollToShortcuts()
        {
            ShowSettings();
            _settingsPane.ScrollToShortcuts();
        }

        public void ShowColorsPlaceholder()
        {
            _settingsPane.SelectColorsTab();
            _pane.Visible = true;
        }

        public void ReloadFromStore()
        {
            _settingsPane.ReloadFromStore();
        }
    }
}
