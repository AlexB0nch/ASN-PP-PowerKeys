using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// View and print HostScript commands (Office.js None unlocks on Windows COM).
    /// Parity with Web Add-in degradation paths in <c>unsupportedWebCommands.ts</c>.
    /// </summary>
    public static class ViewPrintCommands
    {
        public static bool IsViewPrintCommand(CommandIds command) =>
            command == CommandIds.ToggleZoom
            || command == CommandIds.ToggleSlideSorter
            || command == CommandIds.StartSlideShow
            || command == CommandIds.ToggleGrid
            || command == CommandIds.ToggleGuides
            || command == CommandIds.PrintSlide;
    }
}
