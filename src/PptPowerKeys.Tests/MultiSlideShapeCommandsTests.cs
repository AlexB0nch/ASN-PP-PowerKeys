using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class MultiSlideShapeCommandsTests
{
    [Theory]
    [InlineData(CommandIds.PasteShapeToSelectedSlides)]
    [InlineData(CommandIds.RemoveShapeFromSelectedSlides)]
    public void IsMultiSlideShapeCommand_returns_true_for_multislide_shape_commands(CommandIds command)
    {
        Assert.True(MultiSlideShapeCommands.IsMultiSlideShapeCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.Group)]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.DuplicateRight)]
    [InlineData(CommandIds.InsertRectangle)]
    [InlineData(CommandIds.CopyObjectPosition)]
    [InlineData(CommandIds.None)]
    public void IsMultiSlideShapeCommand_returns_false_for_non_multislide_shape_commands(CommandIds command)
    {
        Assert.False(MultiSlideShapeCommands.IsMultiSlideShapeCommand(command));
    }
}
