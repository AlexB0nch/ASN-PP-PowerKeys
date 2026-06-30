using System;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Layout;

namespace PptPowerKeys.Windows.UI
{
    /// <summary>
    /// Maps ribbon control ids (<c>btn{CommandIds}</c>) to layout <see cref="CommandIds"/>.
    /// S08-003: all 32 <see cref="LayoutEngine.IsLayoutCommand"/> ribbon buttons.
    /// </summary>
    public static class RibbonCommandMap
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

            return LayoutEngine.IsLayoutCommand(command);
        }
    }
}
