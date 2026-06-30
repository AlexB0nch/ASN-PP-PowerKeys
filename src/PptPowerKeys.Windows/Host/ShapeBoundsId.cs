using Microsoft.Office.Interop.PowerPoint;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Stable string id for <see cref="PptPowerKeys.Core.Geometry.ShapeBounds"/> round-trip.
    /// PowerPoint COM exposes a numeric <see cref="Shape.Id"/> unique within the presentation.
    /// </summary>
    public static class ShapeBoundsId
    {
        public static string FromComShape(Shape shape) => shape.Id.ToString();

        public static bool TryParse(string id, out int comShapeId) =>
            int.TryParse(id, out comShapeId);
    }
}
