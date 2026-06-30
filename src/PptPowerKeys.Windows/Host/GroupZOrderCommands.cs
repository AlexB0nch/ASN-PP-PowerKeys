using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Group / Ungroup / Z-order HostScript commands. Parity with Web Add-in
    /// <c>groupSelectedShapes</c>, <c>ungroupSelectedShape</c>, <c>setZOrder</c> in
    /// <c>powerpoint.ts</c> and <c>runCommand.ts</c>.
    /// </summary>
    public static class GroupZOrderCommands
    {
        public static bool IsGroupZOrderCommand(CommandIds command) =>
            command == CommandIds.Group
            || command == CommandIds.Ungroup
            || command == CommandIds.BringToFront
            || command == CommandIds.SendToBack
            || command == CommandIds.BringForward
            || command == CommandIds.SendBackward;
    }
}
