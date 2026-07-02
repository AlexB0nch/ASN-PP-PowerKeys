using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class FormatRegroupCommandsTests
{
    [Theory]
    [InlineData(CommandIds.FormatPainter)]
    [InlineData(CommandIds.PasteFormatted)]
    [InlineData(CommandIds.Regroup)]
    public void IsFormatRegroupCommand_returns_true_for_format_regroup_commands(CommandIds command)
    {
        Assert.True(FormatRegroupCommands.IsFormatRegroupCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.Group)]
    [InlineData(CommandIds.PasteUnformatted)]
    [InlineData(CommandIds.FillColor)]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.PrintSlide)]
    public void IsFormatRegroupCommand_returns_false_for_non_format_regroup_commands(CommandIds command)
    {
        Assert.False(FormatRegroupCommands.IsFormatRegroupCommand(command));
    }
}
