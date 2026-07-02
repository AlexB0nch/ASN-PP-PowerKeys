using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Settings task-pane commands (shortcut manager, color scheme, reset to defaults).
    /// S10-004: routed via <see cref="ITaskPaneService"/> — not HostScript or ServerLayout.
    /// </summary>
    public static class SettingsCommands
    {
        public static bool IsSettingsCommand(CommandIds command) =>
            command == CommandIds.OpenShortcutManager
            || command == CommandIds.OpenColorScheme
            || command == CommandIds.ResetToDefaults;
    }
}
