namespace PptPowerKeys.Windows.Settings
{
    /// <summary>
    /// Shows and drives the WPF Settings custom task pane (S10-004).
    /// </summary>
    public interface ITaskPaneService
    {
        void ShowSettings();

        void ShowSettingsScrollToShortcuts();

        void ShowColorPicker();

        void ReloadFromStore();
    }
}
