using System;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;

namespace PptPowerKeys.Windows.UI
{
    /// <summary>
    /// Maps ribbon control ids (<c>btn{CommandIds}</c>) to HostScript <see cref="CommandIds"/>.
    /// S08-004: Copy-and-align commands (separate from layout-only <see cref="RibbonCommandMap"/>).
    /// </summary>
    public static class HostScriptCommandMap
    {
        private const string ButtonPrefix = "btn";

        public static bool TryParse(string controlId, out CommandIds command)
        {
            command = CommandIds.None;
            if (string.IsNullOrEmpty(controlId)
                || !controlId.StartsWith(ButtonPrefix, StringComparison.Ordinal)
                || controlId.Length <= ButtonPrefix.Length)
            {
                return false;
            }

            var suffix = controlId.Substring(ButtonPrefix.Length);
            if (!Enum.TryParse(suffix, ignoreCase: false, out command))
            {
                return false;
            }

            return CopyAndAlignCommands.IsCopyAndAlign(command);
        }
    }
}
