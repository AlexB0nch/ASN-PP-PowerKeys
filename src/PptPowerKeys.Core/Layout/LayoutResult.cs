using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Core.Layout;

/// <summary>
/// Output of <see cref="LayoutEngine.Apply"/>. <see cref="Shapes"/> contains the
/// full set of shapes in the same order as the request, with updated geometry for
/// any that moved or resized. The host applies these back onto the live shapes by
/// matching <see cref="ShapeBounds.Id"/>.
/// </summary>
public sealed record LayoutResult
{
    public required bool Changed { get; init; }

    public required IReadOnlyList<ShapeBounds> Shapes { get; init; }

    /// <summary>Human readable explanation, primarily for no-op results.</summary>
    public string? Message { get; init; }

    public static LayoutResult NoChange(IReadOnlyList<ShapeBounds> shapes, string message) =>
        new() { Changed = false, Shapes = shapes, Message = message };

    public static LayoutResult Updated(IReadOnlyList<ShapeBounds> shapes) =>
        new() { Changed = true, Shapes = shapes };
}
