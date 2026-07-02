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

        /// <summary>
        /// Reads accent1–6 and dark1/2 + light1/2 from the active presentation Slide Master.
        /// Returns an empty list when unavailable.
        /// </summary>
        IReadOnlyList<string> ReadPresentationThemeColors();

        /// <summary>Applies a solid fill color to all selected shapes.</summary>
        int ApplyFillColor(string hex);

        /// <summary>Applies a line color to all selected shapes.</summary>
        int ApplyLineColor(string hex);

        /// <summary>Applies a font color to selected shapes with text.</summary>
        int ApplyTextColor(string hex);

        /// <summary>
        /// Reads fill, line, or text color from the first selected shape as normalized <c>#RRGGBB</c>.
        /// Throws when nothing is selected or the color is unavailable.
        /// </summary>
        string ReadColorFromSelection(ColorPickSource source);

        /// <summary>Toggles fill between black and white for each selected shape.</summary>
        int ToggleFillBlackWhite();

        /// <summary>Reads text per selected shape; empty string when no text frame.</summary>
        IReadOnlyList<string> ReadSelectedShapeTexts();

        /// <summary>Pastes plain clipboard text into selected shapes with text frames.</summary>
        int PasteUnformattedText();

        /// <summary>Replaces text in selected shapes with <c>"..."</c>.</summary>
        int ReplaceSelectedTextWithEllipsis();

        /// <summary>Toggles superscript on selected shapes; enabling disables subscript.</summary>
        int ToggleSuperscript();

        /// <summary>Toggles subscript on selected shapes; enabling disables superscript.</summary>
        int ToggleSubscript();

        /// <summary>
        /// Duplicates the first selected slide immediately after the source via COM <c>Slide.Duplicate()</c>.
        /// Throws when no slide is selected.
        /// </summary>
        void DuplicateSelectedSlide();

        /// <summary>
        /// Moves each selected slide to the end of the deck (high index first to preserve relative order).
        /// Throws when no slide is selected. Returns the number of slides moved.
        /// </summary>
        int MoveSelectedSlidesToBackup();

        /// <summary>
        /// Toggles zoom between fit-to-window and 100%. Returns <c>true</c> when set to fit.
        /// </summary>
        bool ToggleZoomFit();

        /// <summary>
        /// Toggles between normal and slide sorter view. Returns <c>true</c> when sorter is active.
        /// </summary>
        bool ToggleSlideSorterView();

        /// <summary>Starts a slide show from the current slide.</summary>
        void StartSlideShowFromCurrentSlide();

        /// <summary>Toggles grid lines on the active view. Returns <c>true</c> when enabled.</summary>
        bool ToggleGridLines();

        /// <summary>Toggles drawing guides on the active view. Returns <c>true</c> when enabled.</summary>
        bool ToggleGuides();

        /// <summary>Prints the current slide via COM <c>PrintOut</c>.</summary>
        void PrintCurrentSlide();

        /// <summary>
        /// Copies format from the single selected shape via COM <c>Shape.PickUp()</c>.
        /// Throws when selection is not exactly one shape.
        /// </summary>
        void PickUpFormatFromSelection();

        /// <summary>
        /// Applies picked-up format to each selected shape via COM <c>Shape.Apply()</c>.
        /// Throws when nothing is selected. Returns the number of shapes updated.
        /// </summary>
        int ApplyFormatToSelection();

        /// <summary>
        /// Regroups the current shape selection via COM <c>ShapeRange.Regroup()</c>.
        /// Throws when fewer than two shapes are selected. Returns the pre-regroup count.
        /// </summary>
        int RegroupSelectedShapes();

        /// <summary>Pastes formatted clipboard content into selected shapes with text frames.</summary>
        int PasteFormattedText();
    }
}
