using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Copy-and-align HostScript commands: duplicate selection, then align clones to anchor via Core layout.
    /// Parity with Web Add-in <c>COPY_AND_ALIGN_LAYOUT</c> in <c>runCommand.ts</c>.
    /// </summary>
    public static class CopyAndAlignCommands
    {
        public static bool IsCopyAndAlign(CommandIds command) =>
            command == CommandIds.CopyAndAlignLeft
            || command == CommandIds.CopyAndAlignRight
            || command == CommandIds.CopyAndAlignTop
            || command == CommandIds.CopyAndAlignBottom;

        public static bool TryMapToLayoutCommand(CommandIds command, out CommandIds layoutCommand)
        {
            switch (command)
            {
                case CommandIds.CopyAndAlignLeft:
                    layoutCommand = CommandIds.AlignLeft;
                    return true;
                case CommandIds.CopyAndAlignRight:
                    layoutCommand = CommandIds.AlignRight;
                    return true;
                case CommandIds.CopyAndAlignTop:
                    layoutCommand = CommandIds.AlignTop;
                    return true;
                case CommandIds.CopyAndAlignBottom:
                    layoutCommand = CommandIds.AlignBottom;
                    return true;
                default:
                    layoutCommand = CommandIds.None;
                    return false;
            }
        }
    }
}
