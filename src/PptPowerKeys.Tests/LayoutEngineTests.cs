using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Core.Layout;
using Xunit;

namespace PptPowerKeys.Tests;

public class LayoutEngineTests
{
    private static LayoutRequest Request(CommandIds command, params ShapeBounds[] shapes) =>
        new() { Command = command, Shapes = shapes };

    [Fact]
    public void AlignLeft_MovesNonAnchorShapesToAnchorLeft()
    {
        // anchor is the LAST shape (b).
        var a = new ShapeBounds("a", Left: 100, Top: 10, Width: 50, Height: 20);
        var b = new ShapeBounds("b", Left: 30, Top: 200, Width: 80, Height: 40);

        var result = LayoutEngine.Apply(Request(CommandIds.AlignLeft, a, b));

        Assert.True(result.Changed);
        Assert.Equal(30, result.Shapes[0].Left, 6);   // a moved to anchor.Left
        Assert.Equal(10, result.Shapes[0].Top, 6);    // top unchanged
        Assert.Equal(30, result.Shapes[1].Left, 6);   // anchor unchanged
    }

    [Fact]
    public void AlignRight_AlignsRightEdgesToAnchor()
    {
        var a = new ShapeBounds("a", 0, 0, 50, 20);
        var anchor = new ShapeBounds("anchor", 100, 0, 80, 20); // right = 180

        var result = LayoutEngine.Apply(Request(CommandIds.AlignRight, a, anchor));

        Assert.True(result.Changed);
        Assert.Equal(180 - 50, result.Shapes[0].Left, 6); // a.right == anchor.right
        Assert.Equal(180, result.Shapes[0].Right, 6);
    }

    [Fact]
    public void AlignLeftToRight_PlacesLeftEdgeAtAnchorRight()
    {
        var a = new ShapeBounds("a", 0, 0, 50, 20);
        var anchor = new ShapeBounds("anchor", 100, 0, 80, 20); // right = 180

        var result = LayoutEngine.Apply(Request(CommandIds.AlignLeftToRight, a, anchor));

        Assert.True(result.Changed);
        Assert.Equal(180, result.Shapes[0].Left, 6);
        Assert.Equal(100, result.Shapes[1].Left, 6); // anchor unchanged
    }

    [Fact]
    public void AlignRightToLeft_PlacesRightEdgeAtAnchorLeft()
    {
        var a = new ShapeBounds("a", 200, 0, 50, 20);
        var anchor = new ShapeBounds("anchor", 100, 0, 80, 20); // left = 100

        var result = LayoutEngine.Apply(Request(CommandIds.AlignRightToLeft, a, anchor));

        Assert.True(result.Changed);
        Assert.Equal(50, result.Shapes[0].Left, 6); // a.right == anchor.left
        Assert.Equal(100, result.Shapes[0].Right, 6);
    }

    [Fact]
    public void AlignTopToBottom_PlacesTopEdgeAtAnchorBottom()
    {
        var a = new ShapeBounds("a", 0, 0, 50, 20);
        var anchor = new ShapeBounds("anchor", 0, 100, 80, 40); // bottom = 140

        var result = LayoutEngine.Apply(Request(CommandIds.AlignTopToBottom, a, anchor));

        Assert.True(result.Changed);
        Assert.Equal(140, result.Shapes[0].Top, 6);
        Assert.Equal(100, result.Shapes[1].Top, 6); // anchor unchanged
    }

    [Fact]
    public void AlignBottomToTop_PlacesBottomEdgeAtAnchorTop()
    {
        var a = new ShapeBounds("a", 0, 200, 50, 30);
        var anchor = new ShapeBounds("anchor", 0, 100, 80, 40); // top = 100

        var result = LayoutEngine.Apply(Request(CommandIds.AlignBottomToTop, a, anchor));

        Assert.True(result.Changed);
        Assert.Equal(70, result.Shapes[0].Top, 6); // a.bottom == anchor.top
        Assert.Equal(100, result.Shapes[0].Bottom, 6);
    }

    [Fact]
    public void AlignCenterHorizontal_CentersOnAnchorCenter()
    {
        var a = new ShapeBounds("a", 0, 0, 40, 20);
        var anchor = new ShapeBounds("anchor", 100, 0, 100, 20); // centerX = 150

        var result = LayoutEngine.Apply(Request(CommandIds.AlignCenterHorizontal, a, anchor));

        Assert.Equal(150, result.Shapes[0].CenterX, 6);
    }

    [Fact]
    public void SameWidth_SetsWidthToAnchorWidth_LeavesAnchorUnchanged()
    {
        var a = new ShapeBounds("a", 0, 0, 10, 20);
        var anchor = new ShapeBounds("anchor", 0, 0, 99, 20);

        var result = LayoutEngine.Apply(Request(CommandIds.SameWidth, a, anchor));

        Assert.Equal(99, result.Shapes[0].Width, 6);
        Assert.Equal(99, result.Shapes[1].Width, 6); // anchor stays
    }

    [Fact]
    public void SameWidthKeepAspect_ScalesHeightProportionally()
    {
        var a = new ShapeBounds("a", 0, 0, 50, 100); // aspect 1:2
        var anchor = new ShapeBounds("anchor", 0, 0, 100, 10);

        var result = LayoutEngine.Apply(Request(CommandIds.SameWidthKeepAspect, a, anchor));

        Assert.Equal(100, result.Shapes[0].Width, 6);
        Assert.Equal(200, result.Shapes[0].Height, 6); // height doubled with width
    }

    [Fact]
    public void StretchWidthToLeft_KeepsRightEdge()
    {
        var a = new ShapeBounds("a", 100, 0, 50, 20);     // right = 150
        var anchor = new ShapeBounds("anchor", 20, 0, 10, 20); // left = 20

        var result = LayoutEngine.Apply(Request(CommandIds.StretchWidthToLeft, a, anchor));

        Assert.Equal(20, result.Shapes[0].Left, 6);
        Assert.Equal(150, result.Shapes[0].Right, 6); // right preserved
    }

    [Fact]
    public void DistributeHorizontal_EqualizesGaps()
    {
        // three shapes width 10 each, leftmost at 0, rightmost ends at 100.
        var s1 = new ShapeBounds("s1", 0, 0, 10, 10);
        var s2 = new ShapeBounds("s2", 12, 0, 10, 10);
        var s3 = new ShapeBounds("s3", 90, 0, 10, 10); // right = 100

        var result = LayoutEngine.Apply(Request(CommandIds.DistributeHorizontal, s1, s2, s3));

        Assert.True(result.Changed);
        // span 100, sizes 30, free 70, gap 35. middle shape left = 0 + 10 + 35 = 45.
        var middle = result.Shapes.Single(s => s.Id == "s2");
        Assert.Equal(45, middle.Left, 6);
        // endpoints stay put.
        Assert.Equal(0, result.Shapes.Single(s => s.Id == "s1").Left, 6);
        Assert.Equal(90, result.Shapes.Single(s => s.Id == "s3").Left, 6);
    }

    [Fact]
    public void IncreaseWidthLarge_GrowsEveryShape()
    {
        var a = new ShapeBounds("a", 0, 0, 10, 10);
        var b = new ShapeBounds("b", 0, 0, 20, 10);
        var options = new LayoutOptions { LargeStep = 5 };

        var result = LayoutEngine.Apply(new LayoutRequest
        {
            Command = CommandIds.IncreaseWidthLarge,
            Shapes = new[] { a, b },
            Options = options,
        });

        Assert.Equal(15, result.Shapes[0].Width, 6);
        Assert.Equal(25, result.Shapes[1].Width, 6);
    }

    [Fact]
    public void DecreaseWidth_RespectsMinSize()
    {
        var a = new ShapeBounds("a", 0, 0, 3, 10);
        var options = new LayoutOptions { LargeStep = 100, MinSize = 1 };

        var result = LayoutEngine.Apply(new LayoutRequest
        {
            Command = CommandIds.DecreaseWidthLarge,
            Shapes = new[] { a },
            Options = options,
        });

        Assert.Equal(1, result.Shapes[0].Width, 6);
    }

    [Fact]
    public void Align_WithSingleShape_IsNoOp()
    {
        var a = new ShapeBounds("a", 0, 0, 10, 10);
        var result = LayoutEngine.Apply(Request(CommandIds.AlignLeft, a));

        Assert.False(result.Changed);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public void Distribute_WithTwoShapes_IsNoOp()
    {
        var a = new ShapeBounds("a", 0, 0, 10, 10);
        var b = new ShapeBounds("b", 50, 0, 10, 10);
        var result = LayoutEngine.Apply(Request(CommandIds.DistributeHorizontal, a, b));

        Assert.False(result.Changed);
    }

    [Fact]
    public void NonLayoutCommand_ReturnsNoChange()
    {
        var a = new ShapeBounds("a", 0, 0, 10, 10);
        var b = new ShapeBounds("b", 0, 0, 10, 10);
        var result = LayoutEngine.Apply(Request(CommandIds.FillColor, a, b));

        Assert.False(result.Changed);
    }

    [Fact]
    public void EmptySelection_ReturnsNoChange()
    {
        var result = LayoutEngine.Apply(new LayoutRequest
        {
            Command = CommandIds.AlignLeft,
            Shapes = Array.Empty<ShapeBounds>(),
        });

        Assert.False(result.Changed);
    }

    [Fact]
    public void ExplicitAnchorIndex_OverridesLastShape()
    {
        var a = new ShapeBounds("a", 0, 0, 10, 10);
        var b = new ShapeBounds("b", 100, 0, 10, 10);

        var result = LayoutEngine.Apply(new LayoutRequest
        {
            Command = CommandIds.AlignLeft,
            Shapes = new[] { a, b },
            AnchorIndex = 0, // anchor is 'a'
        });

        Assert.Equal(0, result.Shapes[1].Left, 6); // b moved to a.Left
    }

    [Theory]
    [InlineData(CommandIds.AlignLeft, true)]
    [InlineData(CommandIds.AlignLeftToRight, true)]
    [InlineData(CommandIds.AlignRightToLeft, true)]
    [InlineData(CommandIds.AlignTopToBottom, true)]
    [InlineData(CommandIds.AlignBottomToTop, true)]
    [InlineData(CommandIds.SameWidth, true)]
    [InlineData(CommandIds.DistributeVertical, true)]
    [InlineData(CommandIds.FillColor, false)]
    [InlineData(CommandIds.Group, false)]
    public void IsLayoutCommand_Classifies(CommandIds command, bool expected)
    {
        Assert.Equal(expected, LayoutEngine.IsLayoutCommand(command));
    }
}
