using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.UI;
using Xunit;

namespace PptPowerKeys.Tests;

public class SettingsCommandMapTests
{
    [Theory]
    [InlineData("btnShortcutManager", CommandIds.OpenShortcutManager)]
    [InlineData("btnColorScheme", CommandIds.OpenColorScheme)]
    [InlineData("btnResetDefaults", CommandIds.ResetToDefaults)]
    public void TryParse_returns_true_for_settings_ribbon_ids(string controlId, CommandIds expected)
    {
        Assert.True(SettingsCommandMap.TryParse(controlId, out var command));
        Assert.Equal(expected, command);
    }

    [Theory]
    [InlineData("btnAlignLeft")]
    [InlineData("btnFillColor")]
    [InlineData("")]
    public void TryParse_returns_false_for_non_settings_controls(string controlId)
    {
        Assert.False(SettingsCommandMap.TryParse(controlId, out _));
    }
}
