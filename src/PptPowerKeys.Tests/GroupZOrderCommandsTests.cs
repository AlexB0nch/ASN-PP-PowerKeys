using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class GroupZOrderCommandsTests
{
    [Theory]
    [InlineData(CommandIds.Group)]
    [InlineData(CommandIds.Ungroup)]
    [InlineData(CommandIds.BringToFront)]
    [InlineData(CommandIds.SendToBack)]
    [InlineData(CommandIds.BringForward)]
    [InlineData(CommandIds.SendBackward)]
    public void IsGroupZOrderCommand_returns_true_for_group_zorder_commands(CommandIds command)
    {
        Assert.True(GroupZOrderCommands.IsGroupZOrderCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.Regroup)]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.DuplicateRight)]
    [InlineData(CommandIds.InsertRectangle)]
    [InlineData(CommandIds.CopyObjectPosition)]
    [InlineData(CommandIds.None)]
    public void IsGroupZOrderCommand_returns_false_for_non_group_zorder_commands(CommandIds command)
    {
        Assert.False(GroupZOrderCommands.IsGroupZOrderCommand(command));
    }
}
