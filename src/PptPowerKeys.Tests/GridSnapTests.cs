using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Core.Layout;
using Xunit;

namespace PptPowerKeys.Tests;

public class GridSnapTests
{
    private static readonly double Step = GridSnap.GridStepPoints;

    [Fact]
    public void SnapValue_Zero_StaysZero()
    {
        Assert.Equal(0.0, GridSnap.SnapValue(0.0), 9);
    }

    [Fact]
    public void SnapValue_OnGrid_Unchanged()
    {
        Assert.Equal(Step, GridSnap.SnapValue(Step), 9);
        Assert.Equal(Step * 3, GridSnap.SnapValue(Step * 3), 9);
    }

    [Fact]
    public void SnapValue_OffGrid_RoundsToNearestStep()
    {
        // 2.85 pt is slightly above one 0.1 cm step (~2.8346 pt)
        Assert.Equal(Step, GridSnap.SnapValue(2.85), 6);
    }

    [Fact]
    public void SnapValue_MidPoint_RoundsToNearestNode()
    {
        double halfStep = Step / 2.0;
        // Exactly at midpoint: .NET ToEven rounds to the nearer even multiple (0).
        Assert.Equal(0.0, GridSnap.SnapValue(halfStep), 6);
        // Just above midpoint snaps up to one grid step.
        Assert.Equal(Step, GridSnap.SnapValue(halfStep + 0.001), 6);
        // Just below midpoint stays on lower node.
        Assert.Equal(0.0, GridSnap.SnapValue(halfStep - 0.001), 6);
    }

    [Fact]
    public void Snap_PreservesId_AndSnapsGeometry()
    {
        var shape = new ShapeBounds("shape-1", Left: 2.85, Top: 5.7, Width: 50.3, Height: 20.6);
        var snapped = GridSnap.Snap(shape);

        Assert.Equal("shape-1", snapped.Id);
        Assert.Equal(GridSnap.SnapValue(2.85), snapped.Left, 6);
        Assert.Equal(GridSnap.SnapValue(5.7), snapped.Top, 6);
        Assert.Equal(GridSnap.SnapValue(50.3), snapped.Width, 6);
        Assert.Equal(GridSnap.SnapValue(20.6), snapped.Height, 6);
    }

    [Fact]
    public void Snap_ClampsWidthAndHeightToMinSize()
    {
        var shape = new ShapeBounds("tiny", Left: 0, Top: 0, Width: 0.5, Height: 0.3);
        var snapped = GridSnap.Snap(shape, minSize: 1.0);

        Assert.Equal(1.0, snapped.Width, 6);
        Assert.Equal(1.0, snapped.Height, 6);
    }

    [Fact]
    public void SnapAll_MapsEveryShape()
    {
        var shapes = new[]
        {
            new ShapeBounds("a", 1.1, 2.2, 10.1, 10.2),
            new ShapeBounds("b", 3.3, 4.4, 20.5, 20.6),
        };

        var snapped = GridSnap.SnapAll(shapes);

        Assert.Equal(2, snapped.Count);
        Assert.Equal("a", snapped[0].Id);
        Assert.Equal("b", snapped[1].Id);
        Assert.Equal(GridSnap.SnapValue(1.1), snapped[0].Left, 6);
        Assert.Equal(GridSnap.SnapValue(3.3), snapped[1].Left, 6);
    }
}
