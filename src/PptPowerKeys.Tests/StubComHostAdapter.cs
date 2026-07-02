using System.Collections.Generic;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Windows.Host;

namespace PptPowerKeys.Tests;

/// <summary>Minimal COM host stub for CommandRouter routability tests (no PowerPoint).</summary>
internal sealed class StubComHostAdapter : IComHostAdapter
{
    public IReadOnlyList<ShapeBounds> ReadSelectedShapeBounds() => Array.Empty<ShapeBounds>();

    public void ApplyShapeBounds(IReadOnlyList<ShapeBounds> bounds)
    {
    }

    public IReadOnlyList<ShapeBounds> CloneSelectedAtSourcePositions() => Array.Empty<ShapeBounds>();

    public void ApplyShapeBoundsOnSlide(IReadOnlyList<ShapeBounds> bounds)
    {
    }

    public int ApplyPositionToSelection(double left, double top) => 0;

    public void InsertShape(CommandIds command)
    {
    }

    public int DuplicateSelectedAtPositions(
        IReadOnlyList<ShapeBounds> sources,
        IReadOnlyList<ShapeBounds> targets) => 0;

    public int GroupSelectedShapes() => 0;

    public void UngroupSelectedShape()
    {
    }

    public int ApplyZOrderToSelection(CommandIds command) => 0;

    public int PasteShapeToSelectedSlides() => 0;

    public (int SlidesProcessed, int ShapesRemoved) RemoveShapeFromSelectedSlides() => (0, 0);

    public IReadOnlyList<string> ReadPresentationThemeColors() => Array.Empty<string>();

    public int ApplyFillColor(string hex) => 0;

    public int ApplyLineColor(string hex) => 0;

    public int ApplyTextColor(string hex) => 0;

    public int ToggleFillBlackWhite() => 0;

    public IReadOnlyList<string> ReadSelectedShapeTexts() => Array.Empty<string>();

    public int PasteUnformattedText() => 0;

    public int ReplaceSelectedTextWithEllipsis() => 0;

    public int ToggleSuperscript() => 0;

    public int ToggleSubscript() => 0;

    public void DuplicateSelectedSlide()
    {
    }

    public int MoveSelectedSlidesToBackup() => 0;

    public bool ToggleZoomFit() => false;

    public bool ToggleSlideSorterView() => false;

    public void StartSlideShowFromCurrentSlide()
    {
    }

    public bool ToggleGridLines() => false;

    public bool ToggleGuides() => false;

    public void PrintCurrentSlide()
    {
    }

    public void PickUpFormatFromSelection()
    {
    }

    public int ApplyFormatToSelection() => 0;

    public int RegroupSelectedShapes() => 0;

    public int PasteFormattedText() => 0;
}
