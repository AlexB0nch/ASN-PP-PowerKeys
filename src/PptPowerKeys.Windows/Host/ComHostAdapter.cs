using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Colors;
using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Maps PowerPoint COM selection to <see cref="ShapeBounds"/> and writes results back.
    /// Anchor semantics: shapes are returned in selection order; Core uses the last as anchor.
    /// </summary>
    public sealed class ComHostAdapter : IComHostAdapter
    {
        private readonly Application _application;

        public ComHostAdapter(Application application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public IReadOnlyList<ShapeBounds> ReadSelectedShapeBounds()
        {
            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionShapes)
            {
                return Array.Empty<ShapeBounds>();
            }

            ShapeRange range = selection.ShapeRange;
            if (range == null || range.Count < 1)
            {
                return Array.Empty<ShapeBounds>();
            }

            var shapes = new List<ShapeBounds>(range.Count);
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                shapes.Add(ToShapeBounds(shape));
            }

            return shapes;
        }

        public void ApplyShapeBounds(IReadOnlyList<ShapeBounds> bounds)
        {
            if (bounds == null || bounds.Count == 0)
            {
                return;
            }

            var byId = IndexBoundsById(bounds);

            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionShapes)
            {
                return;
            }

            ShapeRange range = selection.ShapeRange;
            if (range == null)
            {
                return;
            }

            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                string id = ShapeBoundsId.FromComShape(shape);
                if (byId.TryGetValue(id, out ShapeBounds target))
                {
                    ApplyBoundsToShape(shape, target);
                }
            }
        }

        public IReadOnlyList<ShapeBounds> CloneSelectedAtSourcePositions()
        {
            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionShapes)
            {
                return Array.Empty<ShapeBounds>();
            }

            ShapeRange range = selection.ShapeRange;
            if (range == null || range.Count < 1)
            {
                return Array.Empty<ShapeBounds>();
            }

            var clones = new List<ShapeBounds>(range.Count);
            for (int i = 1; i <= range.Count; i++)
            {
                Shape source = range[i];
                float sourceLeft = source.Left;
                float sourceTop = source.Top;

                ShapeRange duplicated = source.Duplicate();
                if (duplicated == null || duplicated.Count < 1)
                {
                    continue;
                }

                Shape clone = duplicated[1];
                clone.Left = sourceLeft;
                clone.Top = sourceTop;
                clones.Add(ToShapeBounds(clone));
            }

            return clones;
        }

        public void ApplyShapeBoundsOnSlide(IReadOnlyList<ShapeBounds> bounds)
        {
            if (bounds == null || bounds.Count == 0)
            {
                return;
            }

            Slide slide = GetActiveSlide();
            if (slide == null)
            {
                return;
            }

            var byId = IndexBoundsById(bounds);
            Shapes shapes = slide.Shapes;
            for (int i = 1; i <= shapes.Count; i++)
            {
                Shape shape = shapes[i];
                string id = ShapeBoundsId.FromComShape(shape);
                if (byId.TryGetValue(id, out ShapeBounds target))
                {
                    ApplyBoundsToShape(shape, target);
                }
            }
        }

        public int ApplyPositionToSelection(double left, double top)
        {
            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionShapes)
            {
                return 0;
            }

            ShapeRange range = selection.ShapeRange;
            if (range == null || range.Count < 1)
            {
                return 0;
            }

            float leftPt = (float)left;
            float topPt = (float)top;
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                shape.Left = leftPt;
                shape.Top = topPt;
            }

            return range.Count;
        }

        public void InsertShape(CommandIds command)
        {
            Slide slide = GetActiveSlide();
            if (slide == null)
            {
                throw new InvalidOperationException("No active slide. Open a slide in Normal view and try again.");
            }

            Shapes shapes = slide.Shapes;
            switch (command)
            {
                case CommandIds.InsertRectangle:
                    shapes.AddShape(MsoAutoShapeType.msoShapeRectangle, 100f, 100f, 150f, 100f);
                    break;
                case CommandIds.InsertSquare:
                    shapes.AddShape(MsoAutoShapeType.msoShapeRectangle, 100f, 100f, 100f, 100f);
                    break;
                case CommandIds.InsertEllipse:
                    shapes.AddShape(MsoAutoShapeType.msoShapeOval, 100f, 100f, 150f, 100f);
                    break;
                case CommandIds.InsertLine:
                    shapes.AddLine(100f, 150f, 250f, 150f);
                    break;
                case CommandIds.InsertTextbox:
                    shapes.AddTextbox(
                        MsoTextOrientation.msoTextOrientationHorizontal,
                        100f,
                        100f,
                        200f,
                        80f);
                    break;
                case CommandIds.InsertArrow:
                    Shape arrow = shapes.AddLine(100f, 150f, 250f, 150f);
                    arrow.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, "Not an insert-shape command.");
            }
        }

        public int DuplicateSelectedAtPositions(
            IReadOnlyList<ShapeBounds> sources,
            IReadOnlyList<ShapeBounds> targets)
        {
            if (sources == null || targets == null || sources.Count == 0 || sources.Count != targets.Count)
            {
                return 0;
            }

            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionShapes)
            {
                return 0;
            }

            ShapeRange range = selection.ShapeRange;
            if (range == null || range.Count < 1)
            {
                return 0;
            }

            var byId = new Dictionary<string, Shape>(range.Count, StringComparer.Ordinal);
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                byId[ShapeBoundsId.FromComShape(shape)] = shape;
            }

            int duplicated = 0;
            for (int i = 0; i < sources.Count; i++)
            {
                if (!byId.TryGetValue(sources[i].Id, out Shape source))
                {
                    continue;
                }

                ShapeRange duplicatedRange = source.Duplicate();
                if (duplicatedRange == null || duplicatedRange.Count < 1)
                {
                    continue;
                }

                Shape clone = duplicatedRange[1];
                clone.Left = (float)targets[i].Left;
                clone.Top = (float)targets[i].Top;
                duplicated++;
            }

            return duplicated;
        }

        public int GroupSelectedShapes()
        {
            ShapeRange range = GetSelectedShapeRangeOrEmpty();
            if (range == null || range.Count < 2)
            {
                throw new InvalidOperationException("Select at least two shapes to group.");
            }

            int count = range.Count;
            range.Group();
            return count;
        }

        public void UngroupSelectedShape()
        {
            ShapeRange range = GetSelectedShapeRangeOrEmpty();
            if (range == null || range.Count != 1)
            {
                throw new InvalidOperationException("Select exactly one group to ungroup.");
            }

            Shape shape = range[1];
            if (shape.Type != MsoShapeType.msoGroup)
            {
                throw new InvalidOperationException("Selected shape is not a group.");
            }

            shape.Ungroup();
        }

        public int ApplyZOrderToSelection(CommandIds command)
        {
            MsoZOrderCmd zOrderCmd = command switch
            {
                CommandIds.BringToFront => MsoZOrderCmd.msoBringToFront,
                CommandIds.SendToBack => MsoZOrderCmd.msoSendToBack,
                CommandIds.BringForward => MsoZOrderCmd.msoBringForward,
                CommandIds.SendBackward => MsoZOrderCmd.msoSendBackward,
                _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Not a z-order command."),
            };

            ShapeRange range = GetSelectedShapeRangeOrEmpty();
            if (range == null || range.Count < 1)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            for (int i = 1; i <= range.Count; i++)
            {
                range[i].ZOrder(zOrderCmd);
            }

            return range.Count;
        }

        public int PasteShapeToSelectedSlides()
        {
            SlideRange slideRange = GetSelectedSlideRangeOrThrow(minCount: 2, minCountError: "Select two or more slides first.");

            Shape source = GetExactlyOneShapeOnActiveSlideOrThrow();
            float sourceLeft = source.Left;
            float sourceTop = source.Top;
            float sourceWidth = source.Width;
            float sourceHeight = source.Height;

            Slide sourceSlide = GetActiveSlide();
            if (sourceSlide == null)
            {
                throw new InvalidOperationException("Select exactly one shape on the active slide first.");
            }

            int pastedCount = 0;
            for (int i = 1; i <= slideRange.Count; i++)
            {
                Slide targetSlide = slideRange[i];
                if (targetSlide.SlideIndex == sourceSlide.SlideIndex)
                {
                    continue;
                }

                source.Copy();
                ShapeRange pasted = targetSlide.Shapes.Paste();
                if (pasted == null || pasted.Count < 1)
                {
                    continue;
                }

                Shape pastedShape = pasted[1];
                pastedShape.Left = sourceLeft;
                pastedShape.Top = sourceTop;
                pastedShape.Width = sourceWidth;
                pastedShape.Height = sourceHeight;
                pastedCount++;
            }

            return pastedCount;
        }

        public (int SlidesProcessed, int ShapesRemoved) RemoveShapeFromSelectedSlides()
        {
            SlideRange slideRange = GetSelectedSlideRangeOrThrow(minCount: 1, minCountError: "Select one or more slides first.");

            Shape source = GetExactlyOneShapeOnActiveSlideOrThrow();
            string targetName = source.Name ?? string.Empty;
            if (string.IsNullOrEmpty(targetName))
            {
                throw new InvalidOperationException("Selected shape has no name. Name the shape first.");
            }

            int shapesRemoved = 0;
            int slidesProcessed = slideRange.Count;

            for (int i = 1; i <= slideRange.Count; i++)
            {
                Slide slide = slideRange[i];
                Shapes shapes = slide.Shapes;
                for (int j = shapes.Count; j >= 1; j--)
                {
                    Shape shape = shapes[j];
                    if (string.Equals(shape.Name, targetName, StringComparison.Ordinal))
                    {
                        shape.Delete();
                        shapesRemoved++;
                    }
                }
            }

            return (slidesProcessed, shapesRemoved);
        }

        public IReadOnlyList<string> ReadPresentationThemeColors()
        {
            Presentation? presentation = _application.ActivePresentation;
            if (presentation == null)
            {
                return Array.Empty<string>();
            }

            try
            {
                ThemeColorScheme scheme = presentation.SlideMaster.ThemeColorScheme;
                var colors = new List<string>(ThemeColorSlots.Length);
                foreach (MsoThemeColorIndex slot in ThemeColorSlots)
                {
                    ThemeColor themeColor = scheme.Colors(slot);
                    colors.Add(ColorRgbHelper.OleRgbToHex(themeColor.RGB));
                }

                return colors;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public int ApplyFillColor(string hex)
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select one or more shapes first.");
            int oleRgb = ColorRgbHelper.HexToOleRgb(hex);
            for (int i = 1; i <= range.Count; i++)
            {
                Fill fill = range[i].Fill;
                fill.Visible = MsoTriState.msoTrue;
                fill.Solid();
                fill.ForeColor.RGB = oleRgb;
            }

            return range.Count;
        }

        public int ApplyLineColor(string hex)
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select one or more shapes first.");
            int oleRgb = ColorRgbHelper.HexToOleRgb(hex);
            for (int i = 1; i <= range.Count; i++)
            {
                LineFormat line = range[i].Line;
                line.Visible = MsoTriState.msoTrue;
                line.ForeColor.RGB = oleRgb;
            }

            return range.Count;
        }

        public int ApplyTextColor(string hex)
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select one or more shapes first.");
            int oleRgb = ColorRgbHelper.HexToOleRgb(hex);
            int applied = 0;
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                if (shape.HasTextFrame != MsoTriState.msoTrue)
                {
                    continue;
                }

                try
                {
                    shape.TextFrame.TextRange.Font.Color.RGB = oleRgb;
                    applied++;
                }
                catch
                {
                    // Shape has no usable text frame — skip.
                }
            }

            if (applied == 0)
            {
                throw new InvalidOperationException("Selected shape(s) have no text to color.");
            }

            return applied;
        }

        public string ReadColorFromSelection(ColorPickSource source)
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select a shape first.");
            Shape shape = range[1];
            int oleRgb;
            string label;

            switch (source)
            {
                case ColorPickSource.Fill:
                    oleRgb = shape.Fill.ForeColor.RGB;
                    label = "fill";
                    break;
                case ColorPickSource.Line:
                    oleRgb = shape.Line.ForeColor.RGB;
                    label = "line";
                    break;
                case ColorPickSource.Text:
                    if (shape.HasTextFrame != MsoTriState.msoTrue)
                    {
                        throw new InvalidOperationException("Selected shape has no text frame.");
                    }

                    try
                    {
                        oleRgb = shape.TextFrame.TextRange.Font.Color.RGB;
                    }
                    catch
                    {
                        throw new InvalidOperationException("Selected shape has no text frame.");
                    }

                    label = "text";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown color pick source.");
            }

            string hex = ColorRgbHelper.OleRgbToHex(oleRgb);
            if (!ThemeColor.IsValidHex(hex))
            {
                throw new InvalidOperationException($"No {label} color on selected shape.");
            }

            try
            {
                return ThemeColor.NormalizeHex(hex);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException($"Could not read a valid {label} color from the shape.");
            }
        }

        public int ToggleFillBlackWhite()
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select one or more shapes first.");
            const int blackRgb = 0;
            const int whiteRgb = 0xFFFFFF;
            for (int i = 1; i <= range.Count; i++)
            {
                Fill fill = range[i].Fill;
                int current = fill.ForeColor.RGB;
                fill.Visible = MsoTriState.msoTrue;
                fill.Solid();
                fill.ForeColor.RGB = ColorRgbHelper.IsNearBlackOle(current) ? whiteRgb : blackRgb;
            }

            return range.Count;
        }

        public IReadOnlyList<string> ReadSelectedShapeTexts()
        {
            ShapeRange? range = GetSelectedShapeRangeOrEmpty();
            if (range == null)
            {
                return Array.Empty<string>();
            }

            var texts = new List<string>(range.Count);
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                if (shape.HasTextFrame != MsoTriState.msoTrue)
                {
                    texts.Add(string.Empty);
                    continue;
                }

                try
                {
                    texts.Add(shape.TextFrame.TextRange.Text ?? string.Empty);
                }
                catch
                {
                    texts.Add(string.Empty);
                }
            }

            return texts;
        }

        public int PasteUnformattedText()
        {
            string text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text))
            {
                throw new InvalidOperationException("Clipboard is empty.");
            }

            ShapeRange? range = GetSelectedShapeRangeOrEmpty();
            if (range == null)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            int applied = 0;
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                if (shape.HasTextFrame != MsoTriState.msoTrue)
                {
                    continue;
                }

                try
                {
                    shape.TextFrame.TextRange.Text = text;
                    applied++;
                }
                catch
                {
                    // Shape has no usable text frame — skip.
                }
            }

            if (applied == 0)
            {
                throw new InvalidOperationException("Selected shape(s) have no text frame to paste into.");
            }

            return applied;
        }

        public int ReplaceSelectedTextWithEllipsis()
        {
            ShapeRange? range = GetSelectedShapeRangeOrEmpty();
            if (range == null)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            const string ellipsis = "...";
            int applied = 0;
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                if (shape.HasTextFrame != MsoTriState.msoTrue)
                {
                    continue;
                }

                try
                {
                    shape.TextFrame.TextRange.Text = ellipsis;
                    applied++;
                }
                catch
                {
                    // Shape has no usable text frame — skip.
                }
            }

            if (applied == 0)
            {
                throw new InvalidOperationException("Selected shape(s) have no text to replace.");
            }

            return applied;
        }

        public int ToggleSuperscript() => ToggleScript(superscript: true);

        public int ToggleSubscript() => ToggleScript(superscript: false);

        private int ToggleScript(bool superscript)
        {
            ShapeRange? range = GetSelectedShapeRangeOrEmpty();
            if (range == null)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            var fonts = new List<Microsoft.Office.Interop.PowerPoint.Font>(range.Count);
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                if (shape.HasTextFrame != MsoTriState.msoTrue)
                {
                    continue;
                }

                try
                {
                    fonts.Add(shape.TextFrame.TextRange.Font);
                }
                catch
                {
                    // Shape has no usable text frame — skip.
                }
            }

            if (fonts.Count == 0)
            {
                throw new InvalidOperationException("Selected shape(s) have no text to format.");
            }

            foreach (Microsoft.Office.Interop.PowerPoint.Font font in fonts)
            {
                if (superscript)
                {
                    bool isOn = font.Superscript == MsoTriState.msoTrue;
                    font.Superscript = isOn ? MsoTriState.msoFalse : MsoTriState.msoTrue;
                    if (!isOn)
                    {
                        font.Subscript = MsoTriState.msoFalse;
                    }
                }
                else
                {
                    bool isOn = font.Subscript == MsoTriState.msoTrue;
                    font.Subscript = isOn ? MsoTriState.msoFalse : MsoTriState.msoTrue;
                    if (!isOn)
                    {
                        font.Superscript = MsoTriState.msoFalse;
                    }
                }
            }

            return fonts.Count;
        }

        public void DuplicateSelectedSlide()
        {
            SlideRange slideRange = GetSelectedSlideRangeOrThrow(
                minCount: 1,
                minCountError: "Select a slide first.");

            Slide source = slideRange[1];
            source.Duplicate();
        }

        public int MoveSelectedSlidesToBackup()
        {
            SlideRange slideRange = GetSelectedSlideRangeOrThrow(
                minCount: 1,
                minCountError: "Select one or more slides first.");

            Presentation? presentation = _application.ActivePresentation;
            if (presentation == null)
            {
                throw new InvalidOperationException("Select one or more slides first.");
            }

            var slides = new List<Slide>(slideRange.Count);
            for (int i = 1; i <= slideRange.Count; i++)
            {
                slides.Add(slideRange[i]);
            }

            slides.Sort((a, b) => b.SlideIndex.CompareTo(a.SlideIndex));

            foreach (Slide slide in slides)
            {
                slide.MoveTo(presentation.Slides.Count);
            }

            return slides.Count;
        }

        public bool ToggleZoomFit()
        {
            DocumentWindow window = GetActiveWindowOrThrow("No active window.");
            View view = window.View ?? throw new InvalidOperationException("No active view.");

            int fitZoom = CalculateFitZoom(window, view);
            int currentZoom = view.Zoom;
            bool atFit = Math.Abs(currentZoom - fitZoom) <= 2;

            view.Zoom = atFit ? 100 : fitZoom;
            return !atFit;
        }

        public bool ToggleSlideSorterView()
        {
            DocumentWindow window = GetActiveWindowOrThrow("No active window.");

            bool toSorter = window.ViewType != PpViewType.ppViewSlideSorter;
            window.ViewType = toSorter
                ? PpViewType.ppViewSlideSorter
                : PpViewType.ppViewNormal;

            return toSorter;
        }

        public void StartSlideShowFromCurrentSlide()
        {
            Presentation presentation = _application.ActivePresentation
                ?? throw new InvalidOperationException("Open a presentation first.");

            DocumentWindow window = GetActiveWindowOrThrow("No active window.");
            Slide slide = window.View?.Slide
                ?? throw new InvalidOperationException("No active slide.");

            SlideShowSettings settings = presentation.SlideShowSettings;
            settings.StartingSlide = slide.SlideIndex;
            settings.EndingSlide = presentation.Slides.Count;
            settings.Run();
        }

        public bool ToggleGridLines()
        {
            View view = GetActiveViewOrThrow();
            bool isOn = view.GridLines == MsoTriState.msoTrue;
            view.GridLines = isOn ? MsoTriState.msoFalse : MsoTriState.msoTrue;
            return !isOn;
        }

        public bool ToggleGuides()
        {
            View view = GetActiveViewOrThrow();
            bool isOn = view.Guides == MsoTriState.msoTrue;
            view.Guides = isOn ? MsoTriState.msoFalse : MsoTriState.msoTrue;
            return !isOn;
        }

        public void PrintCurrentSlide()
        {
            Presentation presentation = _application.ActivePresentation
                ?? throw new InvalidOperationException("Open a presentation first.");

            DocumentWindow window = GetActiveWindowOrThrow("No active window.");
            Slide slide = window.View?.Slide
                ?? throw new InvalidOperationException("No active slide.");

            int slideIndex = slide.SlideIndex;
            presentation.PrintOut(
                slideIndex,
                slideIndex,
                string.Empty,
                1,
                MsoTriState.msoFalse,
                MsoTriState.msoFalse,
                MsoTriState.msoTrue,
                Type.Missing,
                MsoTriState.msoFalse,
                MsoTriState.msoFalse);
        }

        public void PickUpFormatFromSelection()
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select exactly one shape to copy format from.");
            if (range.Count != 1)
            {
                throw new InvalidOperationException("Select exactly one shape to copy format from.");
            }

            range[1].PickUp();
        }

        public int ApplyFormatToSelection()
        {
            ShapeRange range = GetSelectedShapeRangeOrThrow("Select one or more shapes to apply format to.");
            int count = range.Count;
            for (int i = 1; i <= range.Count; i++)
            {
                range[i].Apply();
            }

            return count;
        }

        public int RegroupSelectedShapes()
        {
            ShapeRange range = GetSelectedShapeRangeOrEmpty();
            if (range == null || range.Count < 2)
            {
                throw new InvalidOperationException("Select at least two shapes to regroup.");
            }

            int count = range.Count;
            range.Regroup();
            return count;
        }

        public int PasteFormattedText()
        {
            if (!ClipboardContainsPasteableText())
            {
                throw new InvalidOperationException("Clipboard is empty.");
            }

            ShapeRange? range = GetSelectedShapeRangeOrEmpty();
            if (range == null)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            int applied = 0;
            for (int i = 1; i <= range.Count; i++)
            {
                Shape shape = range[i];
                if (shape.HasTextFrame != MsoTriState.msoTrue)
                {
                    continue;
                }

                try
                {
                    shape.TextFrame.TextRange.Paste();
                    applied++;
                }
                catch
                {
                    // Shape has no usable text frame — skip.
                }
            }

            if (applied == 0)
            {
                throw new InvalidOperationException("Selected shape(s) have no text frame to paste into.");
            }

            return applied;
        }

        private static bool ClipboardContainsPasteableText()
        {
            if (Clipboard.ContainsText())
            {
                return true;
            }

            IDataObject? data = Clipboard.GetDataObject();
            if (data == null)
            {
                return false;
            }

            return data.GetDataPresent(DataFormats.Rtf)
                || data.GetDataPresent(DataFormats.Html)
                || data.GetDataPresent(DataFormats.UnicodeText);
        }

        private static int CalculateFitZoom(DocumentWindow window, View view)
        {
            Slide slide = view.Slide;
            if (slide == null)
            {
                return 100;
            }

            float slideWidth = slide.Master.Width;
            float slideHeight = slide.Master.Height;
            if (slideWidth <= 0 || slideHeight <= 0)
            {
                return 100;
            }

            // Reserve space for slide pane chrome (thumbnail strip / scroll bars).
            const float chromeWidth = 40f;
            const float chromeHeight = 115f;
            float availableWidth = Math.Max(1f, window.Width - chromeWidth);
            float availableHeight = Math.Max(1f, window.Height - chromeHeight);

            float widthRatio = availableWidth / slideWidth;
            float heightRatio = availableHeight / slideHeight;
            float zoom = Math.Min(widthRatio, heightRatio) * 100f;

            return (int)Math.Max(10, Math.Min(400, Math.Round(zoom)));
        }

        private DocumentWindow GetActiveWindowOrThrow(string message)
        {
            DocumentWindow window = _application.ActiveWindow;
            if (window == null)
            {
                throw new InvalidOperationException(message);
            }

            return window;
        }

        private View GetActiveViewOrThrow()
        {
            DocumentWindow window = GetActiveWindowOrThrow("No active window.");
            return window.View ?? throw new InvalidOperationException("No active view.");
        }

        private static readonly MsoThemeColorIndex[] ThemeColorSlots =
        {
            MsoThemeColorIndex.msoThemeColorAccent1,
            MsoThemeColorIndex.msoThemeColorAccent2,
            MsoThemeColorIndex.msoThemeColorAccent3,
            MsoThemeColorIndex.msoThemeColorAccent4,
            MsoThemeColorIndex.msoThemeColorAccent5,
            MsoThemeColorIndex.msoThemeColorAccent6,
            MsoThemeColorIndex.msoThemeColorDark1,
            MsoThemeColorIndex.msoThemeColorDark2,
            MsoThemeColorIndex.msoThemeColorLight1,
            MsoThemeColorIndex.msoThemeColorLight2,
        };

        private ShapeRange GetSelectedShapeRangeOrThrow(string emptySelectionError)
        {
            ShapeRange? range = GetSelectedShapeRangeOrEmpty();
            if (range == null || range.Count < 1)
            {
                throw new InvalidOperationException(emptySelectionError);
            }

            return range;
        }

        private SlideRange GetSelectedSlideRangeOrThrow(int minCount, string minCountError)
        {
            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionSlides)
            {
                throw new InvalidOperationException(minCountError);
            }

            SlideRange slideRange = selection.SlideRange;
            if (slideRange == null || slideRange.Count < minCount)
            {
                throw new InvalidOperationException(minCountError);
            }

            return slideRange;
        }

        private Shape GetExactlyOneShapeOnActiveSlideOrThrow()
        {
            ShapeRange range = GetSelectedShapeRangeOrEmpty();
            if (range == null || range.Count != 1)
            {
                throw new InvalidOperationException("Select exactly one shape on the active slide first.");
            }

            return range[1];
        }

        private ShapeRange GetSelectedShapeRangeOrEmpty()
        {
            var selection = _application.ActiveWindow?.Selection;
            if (selection == null || selection.Type != PpSelectionType.ppSelectionShapes)
            {
                return null;
            }

            ShapeRange range = selection.ShapeRange;
            if (range == null || range.Count < 1)
            {
                return null;
            }

            return range;
        }

        private Slide GetActiveSlide() => _application.ActiveWindow?.View?.Slide;

        private static Dictionary<string, ShapeBounds> IndexBoundsById(IReadOnlyList<ShapeBounds> bounds)
        {
            var byId = new Dictionary<string, ShapeBounds>(bounds.Count, StringComparer.Ordinal);
            foreach (var bound in bounds)
            {
                byId[bound.Id] = bound;
            }

            return byId;
        }

        private static void ApplyBoundsToShape(Shape shape, ShapeBounds target)
        {
            shape.Left = (float)target.Left;
            shape.Top = (float)target.Top;
            shape.Width = (float)target.Width;
            shape.Height = (float)target.Height;
        }

        internal static ShapeBounds ToShapeBounds(Shape shape)
        {
            return new ShapeBounds(
                ShapeBoundsId.FromComShape(shape),
                shape.Left,
                shape.Top,
                shape.Width,
                shape.Height);
        }
    }
}
