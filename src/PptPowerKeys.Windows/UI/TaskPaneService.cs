using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Office.Core;
using Microsoft.Office.Tools;
using PptPowerKeys.Windows.Host;
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
            IComHostAdapter host,
            FormatColorPaletteProvider paletteProvider,
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

            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (paletteProvider is null)
            {
                throw new ArgumentNullException(nameof(paletteProvider));
            }

            _settingsPane = new SettingsPane(store, host, paletteProvider, onSettingsSaved);
            var hostControl = new ElementHost
            {
                Child = _settingsPane,
                Dock = DockStyle.Fill,
            };

            _pane = panes.Add(hostControl, "PowerKeys Settings");
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

        public void ShowColorPicker()
        {
            _settingsPane.FocusColorPicker();
            _pane.Visible = true;
        }

        public void ReloadFromStore()
        {
            _settingsPane.ReloadFromStore();
            _settingsPane.ReloadColorPicker();
        }
    }
}
