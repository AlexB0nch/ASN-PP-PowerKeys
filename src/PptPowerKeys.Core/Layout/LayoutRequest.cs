using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Core.Layout;

/// <summary>
/// Input for <see cref="LayoutEngine.Apply"/>: the command to run, the current
/// geometry of the selected shapes (in selection order) and the options to use.
/// </summary>
public sealed record LayoutRequest
{
    public required CommandIds Command { get; init; }

    /// <summary>
    /// Selected shapes in selection order. By PptPowerKeys convention the
    /// <em>last</em> shape in the selection is the anchor for alignment/resize
    /// operations, unless <see cref="AnchorIndex"/> overrides it.
    /// </summary>
    public required IReadOnlyList<ShapeBounds> Shapes { get; init; }

    public LayoutOptions Options { get; init; } = LayoutOptions.Default;

    /// <summary>
    /// Optional explicit anchor index. When null the last shape in
    /// <see cref="Shapes"/> is used as the anchor.
    /// </summary>
    public int? AnchorIndex { get; init; }
}
