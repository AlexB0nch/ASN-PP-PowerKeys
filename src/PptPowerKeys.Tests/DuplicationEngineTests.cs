using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Core.Layout;
using Xunit;

namespace PptPowerKeys.Tests;

public class DuplicationEngineTests
{
    private static readonly ShapeBounds Source = new("a", Left: 100, Top: 100, Width: 40, Height: 20);

    [Fact]
    public void DuplicateRight_OffsetsByWidth()
    {
        var dup = DuplicationEngine.ComputeDuplicate(CommandIds.DuplicateRight, Source);
        Assert.NotNull(dup);
        Assert.Equal(140, dup!.Value.Left, 6);
        Assert.Equal(100, dup.Value.Top, 6);
    }

    [Fact]
    public void DuplicateDown_OffsetsByHeightPlusGap()
    {
        var dup = DuplicationEngine.ComputeDuplicate(CommandIds.DuplicateDown, Source, gap: 5);
        Assert.NotNull(dup);
        Assert.Equal(100, dup!.Value.Left, 6);
        Assert.Equal(125, dup.Value.Top, 6);
    }

    [Fact]
    public void NonDuplicateCommand_ReturnsNull()
    {
        Assert.Null(DuplicationEngine.ComputeDuplicate(CommandIds.AlignLeft, Source));
    }

    [Theory]
    [InlineData(CommandIds.DuplicateLeft, true)]
    [InlineData(CommandIds.DuplicateUp, true)]
    [InlineData(CommandIds.AlignLeft, false)]
    public void IsDuplicateCommand_Classifies(CommandIds command, bool expected)
    {
        Assert.Equal(expected, DuplicationEngine.IsDuplicateCommand(command));
    }

    [Theory]
    [InlineData(CommandIds.DuplicateRight)]
    [InlineData(CommandIds.DuplicateLeft)]
    [InlineData(CommandIds.DuplicateDown)]
    [InlineData(CommandIds.DuplicateUp)]
    public void InferGap_RoundTripsWithComputeDuplicate(CommandIds command)
    {
        const double gap = 5;
        var target = DuplicationEngine.ComputeDuplicate(command, Source, gap);
        Assert.NotNull(target);

        var inferred = DuplicationEngine.InferGap(command, Source, target!.Value);
        Assert.NotNull(inferred);
        Assert.Equal(gap, inferred!.Value, 6);
    }

    [Fact]
    public void InferGap_TouchingDuplicate_ReturnsZero()
    {
        var target = DuplicationEngine.ComputeDuplicate(CommandIds.DuplicateRight, Source);
        Assert.NotNull(target);

        var inferred = DuplicationEngine.InferGap(CommandIds.DuplicateRight, Source, target!.Value);
        Assert.NotNull(inferred);
        Assert.Equal(0, inferred!.Value, 6);
    }

    [Fact]
    public void InferGap_NonDuplicateCommand_ReturnsNull()
    {
        var target = new ShapeBounds("b", Left: 200, Top: 100, Width: 40, Height: 20);
        Assert.Null(DuplicationEngine.InferGap(CommandIds.AlignLeft, Source, target));
    }
}
