using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Core.Layout;

/// <summary>
/// Pure, host-independent implementation of every geometry transformation that
/// PptPowerKeys performs: alignment, distribution and resizing relative to an
/// anchor shape.
/// </summary>
/// <remarks>
/// <para>
/// This is the single most valuable piece of business logic extracted from the
/// VSTO add-in. In the legacy code these operations were intended to be written
/// directly against <c>Microsoft.Office.Interop.PowerPoint.Shape</c> objects,
/// which made them impossible to unit test. Here they operate purely on
/// <see cref="ShapeBounds"/> values, so they can be tested without PowerPoint and
/// reused unchanged from the ASP.NET Core backend.
/// </para>
/// <para>
/// PptPowerKeys aligns and resizes relative to the <em>anchor</em> — the last
/// shape in the selection — rather than the slide edges. This matches the
/// behaviour described in the product README.
/// </para>
/// </remarks>
public static class LayoutEngine
{
    public static LayoutResult Apply(LayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Shapes);

        var shapes = request.Shapes;
        var options = request.Options ?? LayoutOptions.Default;

        if (shapes.Count == 0)
        {
            return LayoutResult.NoChange(shapes, "No shapes selected.");
        }

        var result = request.Command switch
        {
            // Alignment relative to the anchor.
            CommandIds.AlignLeft => AlignEach(request, s => s with { Left = Anchor(request).Left }),
            CommandIds.AlignRight => AlignEach(request, s => s with { Left = Anchor(request).Right - s.Width }),
            CommandIds.AlignCenterHorizontal => AlignEach(request, s => s with { Left = Anchor(request).CenterX - (s.Width / 2.0) }),
            CommandIds.AlignTop => AlignEach(request, s => s with { Top = Anchor(request).Top }),
            CommandIds.AlignBottom => AlignEach(request, s => s with { Top = Anchor(request).Bottom - s.Height }),
            CommandIds.AlignMiddleVertical => AlignEach(request, s => s with { Top = Anchor(request).CenterY - (s.Height / 2.0) }),

            CommandIds.AlignLeftToRight => AlignEach(request, s => s with { Left = Anchor(request).Right }),
            CommandIds.AlignRightToLeft => AlignEach(request, s => s with { Left = Anchor(request).Left - s.Width }),
            CommandIds.AlignTopToBottom => AlignEach(request, s => s with { Top = Anchor(request).Bottom }),
            CommandIds.AlignBottomToTop => AlignEach(request, s => s with { Top = Anchor(request).Top - s.Height }),

            CommandIds.DistributeHorizontal => Distribute(request, horizontal: true),
            CommandIds.DistributeVertical => Distribute(request, horizontal: false),

            // Resize relative to the anchor.
            CommandIds.SameWidth => AlignEach(request, s => s.WithSize(Anchor(request).Width, s.Height)),
            CommandIds.SameHeight => AlignEach(request, s => s.WithSize(s.Width, Anchor(request).Height)),
            CommandIds.SameWidthKeepAspect => AlignEach(request, s => ScaleToWidth(s, Anchor(request).Width)),
            CommandIds.SameHeightKeepAspect => AlignEach(request, s => ScaleToHeight(s, Anchor(request).Height)),
            CommandIds.WidthEqualsAnchorHeight => AlignEach(request, s => s.WithSize(Anchor(request).Height, s.Height)),
            CommandIds.HeightEqualsAnchorWidth => AlignEach(request, s => s.WithSize(s.Width, Anchor(request).Width)),

            CommandIds.StretchWidthToLeft => AlignEach(request, s => StretchHorizontal(s, newLeft: Anchor(request).Left, newRight: s.Right)),
            CommandIds.StretchWidthToRight => AlignEach(request, s => StretchHorizontal(s, newLeft: s.Left, newRight: Anchor(request).Right)),
            CommandIds.StretchHeightToTop => AlignEach(request, s => StretchVertical(s, newTop: Anchor(request).Top, newBottom: s.Bottom)),
            CommandIds.StretchHeightToBottom => AlignEach(request, s => StretchVertical(s, newTop: s.Top, newBottom: Anchor(request).Bottom)),

            // Nudge resize — applies to every selected shape, no anchor needed.
            CommandIds.IncreaseWidthLarge => ResizeAll(request, dw: options.LargeStep, dh: 0, options),
            CommandIds.DecreaseWidthLarge => ResizeAll(request, dw: -options.LargeStep, dh: 0, options),
            CommandIds.IncreaseHeightLarge => ResizeAll(request, dw: 0, dh: options.LargeStep, options),
            CommandIds.DecreaseHeightLarge => ResizeAll(request, dw: 0, dh: -options.LargeStep, options),
            CommandIds.IncreaseWidthSmall => ResizeAll(request, dw: options.SmallStep, dh: 0, options),
            CommandIds.DecreaseWidthSmall => ResizeAll(request, dw: -options.SmallStep, dh: 0, options),
            CommandIds.IncreaseHeightSmall => ResizeAll(request, dw: 0, dh: options.SmallStep, options),
            CommandIds.DecreaseHeightSmall => ResizeAll(request, dw: 0, dh: -options.SmallStep, options),
            CommandIds.IncreaseSizeKeepAspect => ScaleAll(request, delta: options.LargeStep, options),
            CommandIds.DecreaseSizeKeepAspect => ScaleAll(request, delta: -options.LargeStep, options),

            _ => LayoutResult.NoChange(shapes, $"Command '{request.Command}' is not a geometry layout command."),
        };

        return MaybeSnapToGrid(result, options);
    }

    private static LayoutResult MaybeSnapToGrid(LayoutResult result, LayoutOptions options)
    {
        if (!options.SnapToGrid || !result.Changed)
        {
            return result;
        }

        double gridStepPoints = options.GridStepCm * GridSnap.PointsPerCm;
        var snapped = GridSnap.SnapAll(result.Shapes, gridStepPoints, options.MinSize);
        return LayoutResult.Updated(snapped);
    }

    /// <summary>True if the command is a pure geometry transform handled by <see cref="Apply"/>.</summary>
    public static bool IsLayoutCommand(CommandIds command) => command switch
    {
        CommandIds.AlignLeft or CommandIds.AlignRight or CommandIds.AlignCenterHorizontal
            or CommandIds.AlignTop or CommandIds.AlignBottom or CommandIds.AlignMiddleVertical
            or CommandIds.AlignLeftToRight or CommandIds.AlignRightToLeft
            or CommandIds.AlignTopToBottom or CommandIds.AlignBottomToTop
            or CommandIds.DistributeHorizontal or CommandIds.DistributeVertical
            or CommandIds.SameWidth or CommandIds.SameHeight
            or CommandIds.SameWidthKeepAspect or CommandIds.SameHeightKeepAspect
            or CommandIds.WidthEqualsAnchorHeight or CommandIds.HeightEqualsAnchorWidth
            or CommandIds.StretchWidthToLeft or CommandIds.StretchWidthToRight
            or CommandIds.StretchHeightToTop or CommandIds.StretchHeightToBottom
            or CommandIds.IncreaseWidthLarge or CommandIds.DecreaseWidthLarge
            or CommandIds.IncreaseHeightLarge or CommandIds.DecreaseHeightLarge
            or CommandIds.IncreaseWidthSmall or CommandIds.DecreaseWidthSmall
            or CommandIds.IncreaseHeightSmall or CommandIds.DecreaseHeightSmall
            or CommandIds.IncreaseSizeKeepAspect or CommandIds.DecreaseSizeKeepAspect => true,
        _ => false,
    };

    private static ShapeBounds Anchor(LayoutRequest request)
    {
        int index = request.AnchorIndex ?? request.Shapes.Count - 1;
        if (index < 0 || index >= request.Shapes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Anchor index is outside the selection.");
        }

        return request.Shapes[index];
    }

    /// <summary>
    /// Applies a transform to every shape except the anchor. Requires at least two
    /// shapes (anchor + one target).
    /// </summary>
    private static LayoutResult AlignEach(LayoutRequest request, Func<ShapeBounds, ShapeBounds> transform)
    {
        if (request.Shapes.Count < 2)
        {
            return LayoutResult.NoChange(request.Shapes, "Select at least two shapes (the last is the anchor).");
        }

        int anchorIndex = request.AnchorIndex ?? request.Shapes.Count - 1;
        var result = new List<ShapeBounds>(request.Shapes.Count);
        bool changed = false;

        for (int i = 0; i < request.Shapes.Count; i++)
        {
            var shape = request.Shapes[i];
            if (i == anchorIndex)
            {
                result.Add(shape);
                continue;
            }

            var updated = transform(shape);
            if (!ApproximatelyEqual(updated, shape))
            {
                changed = true;
            }

            result.Add(updated);
        }

        return changed ? LayoutResult.Updated(result) : LayoutResult.NoChange(request.Shapes, "Shapes already aligned.");
    }

    private static LayoutResult ResizeAll(LayoutRequest request, double dw, double dh, LayoutOptions options)
    {
        var result = new List<ShapeBounds>(request.Shapes.Count);
        bool changed = false;

        foreach (var shape in request.Shapes)
        {
            double newWidth = Math.Max(options.MinSize, shape.Width + dw);
            double newHeight = Math.Max(options.MinSize, shape.Height + dh);
            var updated = shape.WithSize(newWidth, newHeight);
            if (!ApproximatelyEqual(updated, shape))
            {
                changed = true;
            }

            result.Add(updated);
        }

        return changed ? LayoutResult.Updated(result) : LayoutResult.NoChange(request.Shapes, "No resize possible (minimum size reached).");
    }

    private static LayoutResult ScaleAll(LayoutRequest request, double delta, LayoutOptions options)
    {
        var result = new List<ShapeBounds>(request.Shapes.Count);
        bool changed = false;

        foreach (var shape in request.Shapes)
        {
            if (shape.Width <= 0)
            {
                result.Add(shape);
                continue;
            }

            double newWidth = Math.Max(options.MinSize, shape.Width + delta);
            double factor = newWidth / shape.Width;
            double newHeight = Math.Max(options.MinSize, shape.Height * factor);
            var updated = shape.WithSize(newWidth, newHeight);
            if (!ApproximatelyEqual(updated, shape))
            {
                changed = true;
            }

            result.Add(updated);
        }

        return changed ? LayoutResult.Updated(result) : LayoutResult.NoChange(request.Shapes, "No scaling possible.");
    }

    private static LayoutResult Distribute(LayoutRequest request, bool horizontal)
    {
        var shapes = request.Shapes;
        if (shapes.Count < 3)
        {
            return LayoutResult.NoChange(shapes, "Select at least three shapes to distribute.");
        }

        // Work on indices sorted by the relevant axis so we can write results back
        // in the original order.
        var order = Enumerable.Range(0, shapes.Count)
            .OrderBy(i => horizontal ? shapes[i].Left : shapes[i].Top)
            .ToList();

        var first = shapes[order[0]];
        var last = shapes[order[^1]];

        double span = horizontal
            ? last.Right - first.Left
            : last.Bottom - first.Top;

        double sizeSum = order.Sum(i => horizontal ? shapes[i].Width : shapes[i].Height);
        double freeSpace = span - sizeSum;
        double gap = freeSpace / (shapes.Count - 1);

        var updated = shapes.ToArray();
        double cursor = horizontal ? first.Left : first.Top;
        bool changed = false;

        foreach (int i in order)
        {
            var shape = shapes[i];
            var moved = horizontal ? shape with { Left = cursor } : shape with { Top = cursor };
            if (!ApproximatelyEqual(moved, shape))
            {
                changed = true;
            }

            updated[i] = moved;
            cursor += (horizontal ? shape.Width : shape.Height) + gap;
        }

        return changed ? LayoutResult.Updated(updated) : LayoutResult.NoChange(shapes, "Shapes already evenly distributed.");
    }

    private static ShapeBounds ScaleToWidth(ShapeBounds shape, double targetWidth)
    {
        if (shape.Width <= 0)
        {
            return shape.WithSize(targetWidth, shape.Height);
        }

        double factor = targetWidth / shape.Width;
        return shape.WithSize(targetWidth, shape.Height * factor);
    }

    private static ShapeBounds ScaleToHeight(ShapeBounds shape, double targetHeight)
    {
        if (shape.Height <= 0)
        {
            return shape.WithSize(shape.Width, targetHeight);
        }

        double factor = targetHeight / shape.Height;
        return shape.WithSize(shape.Width * factor, targetHeight);
    }

    private static ShapeBounds StretchHorizontal(ShapeBounds shape, double newLeft, double newRight)
    {
        double left = Math.Min(newLeft, newRight);
        double width = Math.Abs(newRight - newLeft);
        return shape with { Left = left, Width = width };
    }

    private static ShapeBounds StretchVertical(ShapeBounds shape, double newTop, double newBottom)
    {
        double top = Math.Min(newTop, newBottom);
        double height = Math.Abs(newBottom - newTop);
        return shape with { Top = top, Height = height };
    }

    private const double Epsilon = 1e-6;

    private static bool ApproximatelyEqual(ShapeBounds a, ShapeBounds b) =>
        Math.Abs(a.Left - b.Left) < Epsilon &&
        Math.Abs(a.Top - b.Top) < Epsilon &&
        Math.Abs(a.Width - b.Width) < Epsilon &&
        Math.Abs(a.Height - b.Height) < Epsilon;
}
