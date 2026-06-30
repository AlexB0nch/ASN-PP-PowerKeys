using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Insert-shape HostScript commands. Parity with Web Add-in <c>insertShape</c> / <c>insertTextBox</c>
    /// in <c>powerpoint.ts</c> and <c>runCommand.ts</c>.
    /// </summary>
    public static class InsertShapeCommands
    {
        public static bool IsInsertShape(CommandIds command) =>
            command == CommandIds.InsertRectangle
            || command == CommandIds.InsertSquare
            || command == CommandIds.InsertEllipse
            || command == CommandIds.InsertLine
            || command == CommandIds.InsertTextbox
            || command == CommandIds.InsertArrow;
    }
}
