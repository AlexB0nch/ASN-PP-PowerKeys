namespace PptPowerKeys.Commands
{
    public enum CommandIds
    {
        None = 0,

        // Alignment (S01-004)
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

        // Resize (S01-005)
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
        Group,
        Ungroup,
        Regroup,
        SendToBack,
        BringToFront,

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

        // Slides
        ToggleZoom,
        ToggleSlideSorter,
        StartSlideShow,
        ToggleGrid,
        ToggleGuides,
        CopySlide,
        PrintSlide,

        // Settings
        OpenShortcutManager,
        OpenColorScheme,
        ResetToDefaults
    }
}
