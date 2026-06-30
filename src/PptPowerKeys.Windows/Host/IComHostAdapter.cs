using System.Collections.Generic;
using PptPowerKeys.Core.Commands;
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

        /// <summary>
        /// Sets <c>Left</c> and <c>Top</c> for all selected shapes; width and height are unchanged.
        /// Returns the number of shapes updated (0 if nothing selected).
        /// </summary>
        int ApplyPositionToSelection(double left, double top);

        /// <summary>
        /// Inserts a shape on the active slide per <paramref name="command"/>.
        /// Throws <see cref="System.InvalidOperationException"/> when no active slide is available.
        /// </summary>
        void InsertShape(CommandIds command);

        /// <summary>
        /// Clones each source shape and moves the copy to the target left/top.
        /// Width and height of the clone are preserved from the source.
        /// Returns the number of shapes duplicated (0 if none).
        /// </summary>
        int DuplicateSelectedAtPositions(
            IReadOnlyList<ShapeBounds> sources,
            IReadOnlyList<ShapeBounds> targets);

        /// <summary>
        /// Groups the current shape selection via COM <c>ShapeRange.Group()</c>.
        /// Throws when fewer than two shapes are selected. Returns the pre-group count.
        /// </summary>
        int GroupSelectedShapes();

        /// <summary>
        /// Ungroups the single selected group via COM <c>Shape.Ungroup()</c>.
        /// Throws when selection is not exactly one group shape.
        /// </summary>
        void UngroupSelectedShape();

        /// <summary>
        /// Applies COM <c>Shape.ZOrder</c> to each selected shape for the given z-order command.
        /// Throws when nothing is selected. Returns the number of shapes updated.
        /// </summary>
        int ApplyZOrderToSelection(CommandIds command);

        /// <summary>
        /// Copies the single selected shape on the active slide onto every other slide in the
        /// multi-slide selection (skips the source slide). Returns the number of pastes.
        /// </summary>
        int PasteShapeToSelectedSlides();

        /// <summary>
        /// Deletes all shapes matching the selected shape's name on each slide in the selection.
        /// Returns aggregate counts for the status bar.
        /// </summary>
        (int SlidesProcessed, int ShapesRemoved) RemoveShapeFromSelectedSlides();
    }
}
