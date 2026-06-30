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
    }
}
