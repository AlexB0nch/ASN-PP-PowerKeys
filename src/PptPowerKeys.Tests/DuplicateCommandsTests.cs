using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class DuplicateCommandsTests
{
    [Theory]
    [InlineData(CommandIds.DuplicateRight)]
    [InlineData(CommandIds.DuplicateLeft)]
    [InlineData(CommandIds.DuplicateDown)]
    [InlineData(CommandIds.DuplicateUp)]
    public void IsDuplicateCommand_returns_true_for_duplicate_commands(CommandIds command)
    {
        Assert.True(DuplicateCommands.IsDuplicateCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.CopyAndAlignLeft)]
    [InlineData(CommandIds.InsertRectangle)]
    [InlineData(CommandIds.None)]
    public void IsDuplicateCommand_returns_false_for_non_duplicate_commands(CommandIds command)
    {
        Assert.False(DuplicateCommands.IsDuplicateCommand(command));
    }
}
