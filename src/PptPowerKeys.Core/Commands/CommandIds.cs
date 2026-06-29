namespace PptPowerKeys.Core.Commands;

/// <summary>
/// Canonical identifiers for every PptPowerKeys command.
/// Ported verbatim from the legacy VSTO project so that shortcut bindings and
/// settings files remain compatible across the migration. A handful of commands
/// that existed only as ribbon buttons in the legacy UI (e.g. <see cref="InsertArrow"/>)
/// have been promoted to first-class enum values here.
/// </summary>
public enum CommandIds
{
    None = 0,

    // Alignment
    AlignLeft,
    AlignCenterHorizontal,
    AlignRight,
    DistributeHorizontal,
    AlignTop,
    AlignMiddleVertical,
    AlignBottom,
    DistributeVertical,
    CopyAndAlignLeft,
    CopyAndAlignRight,
    CopyAndAlignTop,
    CopyAndAlignBottom,
    AlignLeftToRight,
    AlignRightToLeft,
    AlignTopToBottom,
    AlignBottomToTop,
    CopyObjectPosition,
    PasteObjectPosition,

    // Resize
    SameWidth,
    SameWidthKeepAspect,
    SameHeight,
    SameHeightKeepAspect,
    WidthEqualsAnchorHeight,
    HeightEqualsAnchorWidth,
    StretchWidthToLeft,
    StretchWidthToRight,
    StretchHeightToTop,
    StretchHeightToBottom,
    IncreaseWidthLarge,
    DecreaseWidthLarge,
    IncreaseHeightLarge,
    DecreaseHeightLarge,
    IncreaseSizeKeepAspect,
    DecreaseSizeKeepAspect,
    IncreaseWidthSmall,
    DecreaseWidthSmall,
    IncreaseHeightSmall,
    DecreaseHeightSmall,

    // Objects
    InsertTextbox,
    DuplicateDown,
    DuplicateUp,
    DuplicateRight,
    DuplicateLeft,
    InsertRectangle,
    InsertSquare,
    InsertEllipse,
    InsertLine,
    InsertArrow,
    Group,
    Ungroup,
    Regroup,
    SendToBack,
    BringToFront,
    BringForward,
    SendBackward,

    // Format
    FillColor,
    LineColor,
    TextColor,
    ToggleFillBlackWhite,
    FormatPainter,

    // Text
    PasteUnformatted,
    PasteFormatted,
    AddupTextFields,
    ReplaceWithEllipsis,
    ToggleSuperscript,
    ToggleSubscript,

    // Slides
    ToggleZoom,
    ToggleSlideSorter,
    StartSlideShow,
    ToggleGrid,
    ToggleGuides,
    CopySlide,
    MoveSlidesToBackup,
    PrintSlide,

    // Settings
    OpenShortcutManager,
    OpenColorScheme,
    ResetToDefaults
}
