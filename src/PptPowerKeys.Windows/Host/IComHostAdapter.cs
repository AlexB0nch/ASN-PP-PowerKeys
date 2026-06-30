using System.Collections.Generic;
using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// COM host boundary: read selected shapes into <see cref="ShapeBounds"/>,
    /// apply computed geometry back by <see cref="ShapeBounds.Id"/>.
    /// </summary>
    public interface IComHostAdapter
    {
        IReadOnlyList<ShapeBounds> ReadSelectedShapeBounds();

        void ApplyShapeBounds(IReadOnlyList<ShapeBounds> bounds);

        /// <summary>
        /// Duplicates each selected shape on the active slide at the source position (offset 0).
        /// Returns bounds for the new clones in selection order.
        /// </summary>
        IReadOnlyList<ShapeBounds> CloneSelectedAtSourcePositions();

        /// <summary>
        /// Applies computed geometry to shapes on the active slide by id (not limited to selection).
        /// </summary>
        void ApplyShapeBoundsOnSlide(IReadOnlyList<ShapeBounds> bounds);
    }
}
