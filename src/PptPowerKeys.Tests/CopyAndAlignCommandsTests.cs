using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class CopyAndAlignCommandsTests
{
    [Theory]
    [InlineData(CommandIds.CopyAndAlignLeft, CommandIds.AlignLeft)]
    [InlineData(CommandIds.CopyAndAlignRight, CommandIds.AlignRight)]
    [InlineData(CommandIds.CopyAndAlignTop, CommandIds.AlignTop)]
    [InlineData(CommandIds.CopyAndAlignBottom, CommandIds.AlignBottom)]
    public void TryMapToLayoutCommand_maps_copy_and_align_to_layout(CommandIds copyAndAlign, CommandIds expectedLayout)
    {
        Assert.True(CopyAndAlignCommands.IsCopyAndAlign(copyAndAlign));
        Assert.True(CopyAndAlignCommands.TryMapToLayoutCommand(copyAndAlign, out var layoutCommand));
        Assert.Equal(expectedLayout, layoutCommand);
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.SameWidth)]
    [InlineData(CommandIds.None)]
    public void IsCopyAndAlign_returns_false_for_non_copy_and_align(CommandIds command)
    {
        Assert.False(CopyAndAlignCommands.IsCopyAndAlign(command));
        Assert.False(CopyAndAlignCommands.TryMapToLayoutCommand(command, out _));
    }
}
