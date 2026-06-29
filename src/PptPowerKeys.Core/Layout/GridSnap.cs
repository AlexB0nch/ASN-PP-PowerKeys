using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Core.Layout;

/// <summary>
/// Snaps shape geometry to a fixed grid expressed in centimetres and converted to
/// points (Office.js parity: 72 pt per inch, 2.54 cm per inch).
/// </summary>
public static class GridSnap
{
    public const double GridStepCm = 0.1;

    public const double PointsPerCm = 72.0 / 2.54;

    public static double GridStepPoints => GridStepCm * PointsPerCm;

    /// <summary>Rounds a point value to the nearest grid step.</summary>
    public static double SnapValue(double points, double gridStepPoints = default)
    {
        if (gridStepPoints == default)
        {
            gridStepPoints = GridStepPoints;
        }

        return Math.Round(points / gridStepPoints) * gridStepPoints;
    }

    /// <summary>
    /// Snaps position and size of a shape. Width and height are clamped to at
    /// least <paramref name="minSize"/> after snapping.
    /// </summary>
    public static ShapeBounds Snap(
        ShapeBounds shape,
        double gridStepPoints = default,
        double minSize = 1.0)
    {
        if (gridStepPoints == default)
        {
            gridStepPoints = GridStepPoints;
        }

        double left = SnapValue(shape.Left, gridStepPoints);
        double top = SnapValue(shape.Top, gridStepPoints);
        double width = Math.Max(minSize, SnapValue(shape.Width, gridStepPoints));
        double height = Math.Max(minSize, SnapValue(shape.Height, gridStepPoints));
        return shape with { Left = left, Top = top, Width = width, Height = height };
    }

    /// <summary>Snaps every shape in the collection.</summary>
    public static IReadOnlyList<ShapeBounds> SnapAll(
        IEnumerable<ShapeBounds> shapes,
        double gridStepPoints = default,
        double minSize = 1.0)
    {
        if (shapes is null)
        {
            throw new ArgumentNullException(nameof(shapes));
        }
        return shapes.Select(s => Snap(s, gridStepPoints, minSize)).ToList();
    }
}
