using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Position clipboard HostScript commands (Copy/Paste object position).
    /// Parity with Web Add-in <c>copyObjectPosition</c> / <c>pasteObjectPosition</c>.
    /// </summary>
    public static class PositionCommands
    {
        public static bool IsPositionCommand(CommandIds command) =>
            command == CommandIds.CopyObjectPosition
            || command == CommandIds.PasteObjectPosition;
    }
}
