using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class ViewPrintCommandsTests
{
    [Theory]
    [InlineData(CommandIds.ToggleZoom)]
    [InlineData(CommandIds.ToggleSlideSorter)]
    [InlineData(CommandIds.StartSlideShow)]
    [InlineData(CommandIds.ToggleGrid)]
    [InlineData(CommandIds.ToggleGuides)]
    [InlineData(CommandIds.PrintSlide)]
    public void IsViewPrintCommand_returns_true_for_view_print_commands(CommandIds command)
    {
        Assert.True(ViewPrintCommands.IsViewPrintCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.CopySlide)]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.MoveSlidesToBackup)]
    [InlineData(CommandIds.PasteUnformatted)]
    public void IsViewPrintCommand_returns_false_for_non_view_print_commands(CommandIds command)
    {
        Assert.False(ViewPrintCommands.IsViewPrintCommand(command));
    }
}
