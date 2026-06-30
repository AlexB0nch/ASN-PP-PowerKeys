using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.UI;
using Xunit;

namespace PptPowerKeys.Tests;

public class HostScriptCommandMapTests
{
    [Theory]
    [InlineData("btnCopyAndAlignLeft", CommandIds.CopyAndAlignLeft)]
    [InlineData("btnCopyObjectPosition", CommandIds.CopyObjectPosition)]
    [InlineData("btnPasteObjectPosition", CommandIds.PasteObjectPosition)]
    public void TryParse_returns_true_for_host_script_commands(string controlId, CommandIds expected)
    {
        Assert.True(HostScriptCommandMap.TryParse(controlId, out var command));
        Assert.Equal(expected, command);
    }

    [Theory]
    [InlineData("btnAlignLeft")]
    [InlineData("btnSameWidth")]
    [InlineData("")]
    [InlineData("CopyObjectPosition")]
    public void TryParse_returns_false_for_non_host_script_commands(string controlId)
    {
        Assert.False(HostScriptCommandMap.TryParse(controlId, out _));
    }
}
