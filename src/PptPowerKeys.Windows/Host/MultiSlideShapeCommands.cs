using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Multi-slide paste / remove shape HostScript commands. Parity with Web Add-in
    /// <c>pasteShapeToSelectedSlides</c>, <c>removeShapeFromSelectedSlides</c> in
    /// <c>powerpoint.ts</c> and <c>runCommand.ts</c>.
    /// </summary>
    public static class MultiSlideShapeCommands
    {
        public static bool IsMultiSlideShapeCommand(CommandIds command) =>
            command == CommandIds.PasteShapeToSelectedSlides
            || command == CommandIds.RemoveShapeFromSelectedSlides;
    }
}
