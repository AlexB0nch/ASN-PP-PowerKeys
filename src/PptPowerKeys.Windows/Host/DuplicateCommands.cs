using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Layout;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Smart-duplicate HostScript commands. Parity with Web Add-in <c>Duplicate*</c> cases in
    /// <c>runCommand.ts</c> and Core <see cref="DuplicationEngine"/>.
    /// </summary>
    public static class DuplicateCommands
    {
        public static bool IsDuplicateCommand(CommandIds command) =>
            DuplicationEngine.IsDuplicateCommand(command);
    }
}
