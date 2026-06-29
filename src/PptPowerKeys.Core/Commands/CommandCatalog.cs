using System.Collections.ObjectModel;

namespace PptPowerKeys.Core.Commands;

/// <summary>
/// The authoritative registry of every PptPowerKeys command together with its
/// Office.js feasibility rating. Both the backend API (<c>GET /api/commands</c>)
/// and the task pane consume this so the UI and the migration audit never drift
/// apart.
/// </summary>
public static class CommandCatalog
{
    private static readonly ReadOnlyCollection<CommandDescriptor> _all = BuildAll();

    public static IReadOnlyList<CommandDescriptor> All => _all;

    public static IEnumerable<CommandDescriptor> ByCategory(CommandCategory category) =>
        _all.Where(c => c.Category == category);

    public static CommandDescriptor? Find(CommandIds id) =>
        _all.FirstOrDefault(c => c.Id == id);

    public static CommandDescriptor? Find(string key) =>
        Enum.TryParse<CommandIds>(key, ignoreCase: true, out var id) ? Find(id) : null;

    private static ReadOnlyCollection<CommandDescriptor> BuildAll()
    {
        var list = new List<CommandDescriptor>
        {
            // ── Alignment (anchor = last selected shape) ─────────────────────
            Layout(CommandIds.AlignLeft, "Align left", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+1",
                "setLeft on each shape to anchor.left."),
            Layout(CommandIds.AlignCenterHorizontal, "Align center (H)", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+2"),
            Layout(CommandIds.AlignRight, "Align right", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+3"),
            Layout(CommandIds.AlignTop, "Align top", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+4"),
            Layout(CommandIds.AlignMiddleVertical, "Align middle (V)", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+5"),
            Layout(CommandIds.AlignBottom, "Align bottom", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+6"),
            Layout(CommandIds.DistributeHorizontal, "Distribute horizontally", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+7"),
            Layout(CommandIds.DistributeVertical, "Distribute vertically", CommandCategory.Alignment, OfficeJsSupport.Full, "Alt+8"),
            Host(CommandIds.CopyAndAlignLeft, "Duplicate + align left", CommandCategory.Alignment, OfficeJsSupport.Partial,
                notes: "Duplicate then AlignLeft; combines DuplicationEngine + LayoutEngine."),
            Host(CommandIds.CopyAndAlignRight, "Duplicate + align right", CommandCategory.Alignment, OfficeJsSupport.Partial),
            Host(CommandIds.CopyAndAlignTop, "Duplicate + align top", CommandCategory.Alignment, OfficeJsSupport.Partial),
            Host(CommandIds.CopyAndAlignBottom, "Duplicate + align bottom", CommandCategory.Alignment, OfficeJsSupport.Partial),
            Layout(CommandIds.AlignLeftToRight, "Place to right of anchor", CommandCategory.Alignment, OfficeJsSupport.Full,
                notes: "Position shape so its left edge meets the anchor's right edge."),
            Layout(CommandIds.AlignRightToLeft, "Place to left of anchor", CommandCategory.Alignment, OfficeJsSupport.Full),
            Layout(CommandIds.AlignTopToBottom, "Place below anchor", CommandCategory.Alignment, OfficeJsSupport.Full),
            Layout(CommandIds.AlignBottomToTop, "Place above anchor", CommandCategory.Alignment, OfficeJsSupport.Full),
            Host(CommandIds.CopyObjectPosition, "Copy object position", CommandCategory.Alignment, OfficeJsSupport.Full,
                notes: "Store anchor geometry in task pane state."),
            Host(CommandIds.PasteObjectPosition, "Paste object position", CommandCategory.Alignment, OfficeJsSupport.Full,
                notes: "Apply stored geometry to the selection."),

            // ── Resize relative to anchor ────────────────────────────────────
            Layout(CommandIds.SameWidth, "Same width", CommandCategory.Resize, OfficeJsSupport.Full, "Alt+B"),
            Layout(CommandIds.SameHeight, "Same height", CommandCategory.Resize, OfficeJsSupport.Full, "Alt+H"),
            Layout(CommandIds.SameWidthKeepAspect, "Same width (keep aspect)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.SameHeightKeepAspect, "Same height (keep aspect)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.WidthEqualsAnchorHeight, "Width = anchor height", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.HeightEqualsAnchorWidth, "Height = anchor width", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.StretchWidthToLeft, "Stretch width to anchor left", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.StretchWidthToRight, "Stretch width to anchor right", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.StretchHeightToTop, "Stretch height to anchor top", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.StretchHeightToBottom, "Stretch height to anchor bottom", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.IncreaseWidthLarge, "Increase width (large)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.DecreaseWidthLarge, "Decrease width (large)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.IncreaseHeightLarge, "Increase height (large)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.DecreaseHeightLarge, "Decrease height (large)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.IncreaseWidthSmall, "Increase width (small)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.DecreaseWidthSmall, "Decrease width (small)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.IncreaseHeightSmall, "Increase height (small)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.DecreaseHeightSmall, "Decrease height (small)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.IncreaseSizeKeepAspect, "Increase size (keep aspect)", CommandCategory.Resize, OfficeJsSupport.Full),
            Layout(CommandIds.DecreaseSizeKeepAspect, "Decrease size (keep aspect)", CommandCategory.Resize, OfficeJsSupport.Full),

            // ── Objects ──────────────────────────────────────────────────────
            Host(CommandIds.InsertRectangle, "Insert rectangle", CommandCategory.Objects, OfficeJsSupport.Full,
                notes: "shapes.addGeometricShape(GeometricShapeType.rectangle)."),
            Host(CommandIds.InsertSquare, "Insert square", CommandCategory.Objects, OfficeJsSupport.Full,
                notes: "addGeometricShape then equalize width/height."),
            Host(CommandIds.InsertEllipse, "Insert circle/ellipse", CommandCategory.Objects, OfficeJsSupport.Full,
                notes: "addGeometricShape(GeometricShapeType.ellipse)."),
            Host(CommandIds.InsertLine, "Insert line", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shapes.addLine()."),
            Host(CommandIds.InsertArrow, "Insert arrow", CommandCategory.Objects, OfficeJsSupport.Partial,
                notes: "addLine + set lineFormat arrowhead; arrowhead style support is limited."),
            Host(CommandIds.InsertTextbox, "Insert text box", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shapes.addTextBox()."),
            Host(CommandIds.DuplicateRight, "Duplicate right", CommandCategory.Objects, OfficeJsSupport.Partial,
                notes: "copyTo/duplicate then position via DuplicationEngine; no native shape.duplicate in all builds."),
            Host(CommandIds.DuplicateLeft, "Duplicate left", CommandCategory.Objects, OfficeJsSupport.Partial),
            Host(CommandIds.DuplicateDown, "Duplicate down", CommandCategory.Objects, OfficeJsSupport.Partial),
            Host(CommandIds.DuplicateUp, "Duplicate up", CommandCategory.Objects, OfficeJsSupport.Partial),
            Host(CommandIds.Group, "Group", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shapes.addGroup(...)."),
            Host(CommandIds.Ungroup, "Ungroup", CommandCategory.Objects, OfficeJsSupport.Partial, notes: "group.ungroup() (newer API set)."),
            Host(CommandIds.Regroup, "Regroup", CommandCategory.Objects, OfficeJsSupport.None, notes: "No regroup API; must track membership manually."),
            Host(CommandIds.BringToFront, "Bring to front", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shape.setZOrder(toFront)."),
            Host(CommandIds.SendToBack, "Send to back", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shape.setZOrder(toBack)."),
            Host(CommandIds.BringForward, "Bring forward", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shape.setZOrder(bringForward)."),
            Host(CommandIds.SendBackward, "Send backward", CommandCategory.Objects, OfficeJsSupport.Full, notes: "shape.setZOrder(sendBackward)."),

            // ── Format ───────────────────────────────────────────────────────
            Host(CommandIds.FillColor, "Fill color", CommandCategory.Format, OfficeJsSupport.Partial, "Alt+G",
                "shape.fill.setSolidColor(); theme/slide-master palette read is limited vs COM."),
            Host(CommandIds.LineColor, "Line color", CommandCategory.Format, OfficeJsSupport.Partial,
                notes: "shape.lineFormat.color."),
            Host(CommandIds.TextColor, "Text color", CommandCategory.Format, OfficeJsSupport.Full,
                notes: "textFrame.textRange.font.color."),
            Host(CommandIds.ToggleFillBlackWhite, "Toggle fill black/white", CommandCategory.Format, OfficeJsSupport.Full),
            Host(CommandIds.FormatPainter, "Format painter", CommandCategory.Format, OfficeJsSupport.None,
                notes: "No format-painter API; emulate by copying selected format properties manually."),

            // ── Text ─────────────────────────────────────────────────────────
            Host(CommandIds.PasteUnformatted, "Paste unformatted", CommandCategory.Text, OfficeJsSupport.Partial,
                notes: "Read clipboard text in task pane and insert via textRange; no rich clipboard access."),
            Host(CommandIds.PasteFormatted, "Paste formatted", CommandCategory.Text, OfficeJsSupport.None,
                notes: "No rich clipboard API in Office.js."),
            Host(CommandIds.AddupTextFields, "Sum numeric fields", CommandCategory.Text, OfficeJsSupport.Full,
                notes: "Read text from selected shapes, parse numbers, write total."),
            Host(CommandIds.ReplaceWithEllipsis, "Replace with ellipsis", CommandCategory.Text, OfficeJsSupport.Full),
            Host(CommandIds.ToggleSuperscript, "Toggle superscript", CommandCategory.Text, OfficeJsSupport.Partial,
                notes: "font.superscript exists in newer requirement sets."),
            Host(CommandIds.ToggleSubscript, "Toggle subscript", CommandCategory.Text, OfficeJsSupport.Partial),

            // ── Slides ───────────────────────────────────────────────────────
            Host(CommandIds.ToggleZoom, "Toggle zoom fit", CommandCategory.Slides, OfficeJsSupport.None, "F1",
                "No view-zoom API; blocker."),
            Host(CommandIds.ToggleSlideSorter, "Toggle slide sorter", CommandCategory.Slides, OfficeJsSupport.None,
                notes: "No view-switching API; blocker."),
            Host(CommandIds.StartSlideShow, "Start slide show", CommandCategory.Slides, OfficeJsSupport.None),
            Host(CommandIds.ToggleGrid, "Toggle grid", CommandCategory.Slides, OfficeJsSupport.None),
            Host(CommandIds.ToggleGuides, "Toggle guides", CommandCategory.Slides, OfficeJsSupport.None),
            Host(CommandIds.CopySlide, "Copy/duplicate slide", CommandCategory.Slides, OfficeJsSupport.Partial,
                notes: "presentation.slides has insert/clone in newer API; otherwise workaround."),
            Host(CommandIds.MoveSlidesToBackup, "Move slides to backup (end of deck)", CommandCategory.Slides,
                OfficeJsSupport.Partial,
                notes: "Move to end of deck only; slide sections are not available on PowerPoint Web."),
            Host(CommandIds.PrintSlide, "Print slide", CommandCategory.Slides, OfficeJsSupport.None,
                notes: "No print API; rely on host/browser print."),

            // ── Settings ─────────────────────────────────────────────────────
            SettingsCmd(CommandIds.OpenShortcutManager, "Shortcut manager", OfficeJsSupport.Full),
            SettingsCmd(CommandIds.OpenColorScheme, "Color scheme", OfficeJsSupport.Full),
            SettingsCmd(CommandIds.ResetToDefaults, "Reset to defaults", OfficeJsSupport.Full),
        };

        return new ReadOnlyCollection<CommandDescriptor>(list);
    }

    private static CommandDescriptor Layout(CommandIds id, string title, CommandCategory category, OfficeJsSupport support,
        string? shortcut = null, string? notes = null) => new()
    {
        Id = id,
        Title = title,
        Category = category,
        Execution = ExecutionKind.ServerLayout,
        Support = support,
        DefaultShortcut = shortcut,
        Notes = notes,
    };

    private static CommandDescriptor Host(CommandIds id, string title, CommandCategory category, OfficeJsSupport support,
        string? shortcut = null, string? notes = null) => new()
    {
        Id = id,
        Title = title,
        Category = category,
        Execution = ExecutionKind.HostScript,
        Support = support,
        DefaultShortcut = shortcut,
        Notes = notes,
    };

    private static CommandDescriptor SettingsCmd(CommandIds id, string title, OfficeJsSupport support, string? notes = null) => new()
    {
        Id = id,
        Title = title,
        Category = CommandCategory.Settings,
        Execution = ExecutionKind.Settings,
        Support = support,
        Notes = notes,
    };
}
