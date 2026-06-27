namespace PptPowerKeys.Core.Geometry;

/// <summary>
/// Immutable axis-aligned bounding box for a single shape, expressed in points
/// (the same unit the PowerPoint JavaScript API uses for <c>left</c>/<c>top</c>/
/// <c>width</c>/<c>height</c>). The <see cref="Id"/> mirrors the Office.js shape id
/// so the task pane can map computed results back onto live shapes.
/// </summary>
/// <remarks>
/// This type is deliberately free of any Office Interop dependency. It is the
/// boundary contract between the (untestable) Office host and the (testable)
/// layout engine: the host reads geometry into <see cref="ShapeBounds"/>, the
/// engine computes new geometry, and the host writes it back.
/// </remarks>
public readonly record struct ShapeBounds(string Id, double Left, double Top, double Width, double Height)
{
    public double Right => Left + Width;

    public double Bottom => Top + Height;

    public double CenterX => Left + (Width / 2.0);

    public double CenterY => Top + (Height / 2.0);

    public double Area => Width * Height;

    /// <summary>Returns a copy positioned at the given top-left corner.</summary>
    public ShapeBounds WithPosition(double left, double top) => this with { Left = left, Top = top };

    /// <summary>Returns a copy with the given size (top-left corner unchanged).</summary>
    public ShapeBounds WithSize(double width, double height) => this with { Width = width, Height = height };

    /// <summary>Returns a copy translated by the given offsets.</summary>
    public ShapeBounds Offset(double dx, double dy) => this with { Left = Left + dx, Top = Top + dy };
}
