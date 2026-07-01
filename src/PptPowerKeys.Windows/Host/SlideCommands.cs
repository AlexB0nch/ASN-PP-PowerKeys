using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Slide HostScript commands. Parity with Web Add-in <c>duplicateSelectedSlide</c>,
    /// <c>moveSelectedSlidesToBackup</c> in <c>powerpoint.ts</c> and <c>runCommand.ts</c>.
    /// </summary>
    public static class SlideCommands
    {
        public static bool IsSlideCommand(CommandIds command) =>
            command == CommandIds.CopySlide
            || command == CommandIds.MoveSlidesToBackup;
    }
}
