using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class SettingsCommandsTests
{
    [Theory]
    [InlineData(CommandIds.OpenShortcutManager)]
    [InlineData(CommandIds.OpenColorScheme)]
    [InlineData(CommandIds.ResetToDefaults)]
    public void IsSettingsCommand_returns_true_for_settings_commands(CommandIds command)
    {
        Assert.True(SettingsCommands.IsSettingsCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.FillColor)]
    [InlineData(CommandIds.FormatPainter)]
    [InlineData(CommandIds.Group)]
    public void IsSettingsCommand_returns_false_for_non_settings_commands(CommandIds command)
    {
        Assert.False(SettingsCommands.IsSettingsCommand(command));
    }
}
