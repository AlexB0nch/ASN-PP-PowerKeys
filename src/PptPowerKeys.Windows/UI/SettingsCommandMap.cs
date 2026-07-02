using System;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;

namespace PptPowerKeys.Windows.UI
{
    /// <summary>
    /// Maps ribbon control ids in <c>grpSettings</c> to Settings <see cref="CommandIds"/>.
    /// S10-004: Shortcut manager, color scheme, reset to defaults.
    /// </summary>
    public static class SettingsCommandMap
    {
        public static bool TryParse(string controlId, out CommandIds command)
        {
            command = controlId switch
            {
                "btnShortcutManager" => CommandIds.OpenShortcutManager,
                "btnColorScheme" => CommandIds.OpenColorScheme,
                "btnResetDefaults" => CommandIds.ResetToDefaults,
                _ => CommandIds.None,
            };

            return command != CommandIds.None
                && SettingsCommands.IsSettingsCommand(command);
        }
    }
}
