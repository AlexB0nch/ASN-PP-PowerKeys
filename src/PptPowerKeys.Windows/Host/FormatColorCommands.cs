using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Format color HostScript commands. Parity with Web Add-in
    /// <c>formatColorState.ts</c>, <c>powerpoint.ts</c>, and <c>runCommand.ts</c>.
    /// </summary>
    public static class FormatColorCommands
    {
        public static bool IsFormatColorCommand(CommandIds command) =>
            command == CommandIds.FillColor
            || command == CommandIds.LineColor
            || command == CommandIds.TextColor
            || command == CommandIds.ToggleFillBlackWhite;

        public static bool IsPaletteColorCommand(CommandIds command) =>
            command == CommandIds.FillColor
            || command == CommandIds.LineColor
            || command == CommandIds.TextColor;
    }
}
