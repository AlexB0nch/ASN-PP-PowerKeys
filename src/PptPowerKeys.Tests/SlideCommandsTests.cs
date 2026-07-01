using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class SlideCommandsTests
{
    [Theory]
    [InlineData(CommandIds.CopySlide)]
    [InlineData(CommandIds.MoveSlidesToBackup)]
    public void IsSlideCommand_returns_true_for_slide_commands(CommandIds command)
    {
        Assert.True(SlideCommands.IsSlideCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.PasteShapeToSelectedSlides)]
    [InlineData(CommandIds.PasteUnformatted)]
    public void IsSlideCommand_returns_false_for_non_slide_commands(CommandIds command)
    {
        Assert.False(SlideCommands.IsSlideCommand(command));
    }
}
