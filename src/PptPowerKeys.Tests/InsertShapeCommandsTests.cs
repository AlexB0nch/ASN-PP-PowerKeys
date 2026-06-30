using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class InsertShapeCommandsTests
{
    [Theory]
    [InlineData(CommandIds.InsertRectangle)]
    [InlineData(CommandIds.InsertSquare)]
    [InlineData(CommandIds.InsertEllipse)]
    [InlineData(CommandIds.InsertLine)]
    [InlineData(CommandIds.InsertTextbox)]
    [InlineData(CommandIds.InsertArrow)]
    public void IsInsertShape_returns_true_for_insert_commands(CommandIds command)
    {
        Assert.True(InsertShapeCommands.IsInsertShape(command));
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft)]
    [InlineData(CommandIds.CopyAndAlignLeft)]
    [InlineData(CommandIds.CopyObjectPosition)]
    [InlineData(CommandIds.SameWidth)]
    [InlineData(CommandIds.None)]
    public void IsInsertShape_returns_false_for_non_insert_commands(CommandIds command)
    {
        Assert.False(InsertShapeCommands.IsInsertShape(command));
    }
}
