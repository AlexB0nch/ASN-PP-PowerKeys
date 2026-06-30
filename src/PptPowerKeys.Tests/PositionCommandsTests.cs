using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class PositionCommandsTests
{
    [Theory]
    [InlineData(CommandIds.CopyObjectPosition)]
    [InlineData(CommandIds.PasteObjectPosition)]
    public void IsPositionCommand_returns_true_for_position_commands(CommandIds command)
    {
        Assert.True(PositionCommands.IsPositionCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.CopyAndAlignLeft)]
    [InlineData(CommandIds.None)]
    public void IsPositionCommand_returns_false_for_other_commands(CommandIds command)
    {
        Assert.False(PositionCommands.IsPositionCommand(command));
    }
}
