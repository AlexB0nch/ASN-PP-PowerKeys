using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class FormatColorCommandsTests
{
    [Theory]
    [InlineData(CommandIds.FillColor)]
    [InlineData(CommandIds.LineColor)]
    [InlineData(CommandIds.TextColor)]
    [InlineData(CommandIds.ToggleFillBlackWhite)]
    public void IsFormatColorCommand_returns_true_for_format_commands(CommandIds command)
    {
        Assert.True(FormatColorCommands.IsFormatColorCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.FillColor)]
    [InlineData(CommandIds.LineColor)]
    [InlineData(CommandIds.TextColor)]
    public void IsPaletteColorCommand_returns_true_for_palette_commands(CommandIds command)
    {
        Assert.True(FormatColorCommands.IsPaletteColorCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.ToggleFillBlackWhite)]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.Group)]
    public void IsPaletteColorCommand_returns_false_for_non_palette_commands(CommandIds command)
    {
        Assert.False(FormatColorCommands.IsPaletteColorCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.InsertRectangle)]
    public void IsFormatColorCommand_returns_false_for_non_format_commands(CommandIds command)
    {
        Assert.False(FormatColorCommands.IsFormatColorCommand(command));
    }
}
