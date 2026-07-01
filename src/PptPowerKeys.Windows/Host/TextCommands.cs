using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Text HostScript commands. Parity with Web Add-in <c>powerpoint.ts</c> and <c>runCommand.ts</c>.
    /// </summary>
    public static class TextCommands
    {
        public static bool IsTextCommand(CommandIds command) =>
            command == CommandIds.PasteUnformatted
            || command == CommandIds.ReplaceWithEllipsis
            || command == CommandIds.ToggleSuperscript
            || command == CommandIds.ToggleSubscript
            || command == CommandIds.AddupTextFields;
    }
}
