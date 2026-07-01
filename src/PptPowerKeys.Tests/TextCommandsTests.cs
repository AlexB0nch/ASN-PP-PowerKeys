using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class TextCommandsTests
{
    [Theory]
    [InlineData(CommandIds.PasteUnformatted)]
    [InlineData(CommandIds.ReplaceWithEllipsis)]
    [InlineData(CommandIds.ToggleSuperscript)]
    [InlineData(CommandIds.ToggleSubscript)]
    [InlineData(CommandIds.AddupTextFields)]
    public void IsTextCommand_returns_true_for_text_commands(CommandIds command)
    {
        Assert.True(TextCommands.IsTextCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.FillColor)]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.InsertRectangle)]
    public void IsTextCommand_returns_false_for_non_text_commands(CommandIds command)
    {
        Assert.False(TextCommands.IsTextCommand(command));
    }
}
