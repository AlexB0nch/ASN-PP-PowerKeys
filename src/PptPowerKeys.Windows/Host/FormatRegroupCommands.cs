using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Format painter, formatted paste, and regroup HostScript commands (Office.js None unlocks on Windows COM).
    /// Parity with Web Add-in degradation paths in <c>unsupportedWebCommands.ts</c>.
    /// </summary>
    public static class FormatRegroupCommands
    {
        public static bool IsFormatRegroupCommand(CommandIds command) =>
            command == CommandIds.FormatPainter
            || command == CommandIds.PasteFormatted
            || command == CommandIds.Regroup;
    }
}
