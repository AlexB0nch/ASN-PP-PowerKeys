using Microsoft.Office.Interop.PowerPoint;

namespace PptPowerKeys.Core
{
    /// <summary>
    /// Execution context for a PowerPoint command.
    /// Alignment commands use <see cref="AnchorShape"/> (last selected object), not slide edges.
    /// </summary>
    public class CommandContext
    {
        public CommandContext(Application application)
        {
            Application = application;
            RefreshSelection();
        }

        public Application Application { get; }

        public Selection Selection { get; private set; }

        /// <summary>Last selected shape used as alignment anchor.</summary>
        public Shape AnchorShape { get; private set; }

        public bool HasSelection => AnchorShape != null;

        public void RefreshSelection()
        {
            AnchorShape = null;
            Selection = null;

            if (Application?.ActiveWindow == null)
            {
                return;
            }

            Selection = Application.ActiveWindow.Selection;
            if (Selection?.Type != PpSelectionType.ppSelectionShapes)
            {
                return;
            }

            ShapeRange range = Selection.ShapeRange;
            if (range == null || range.Count < 1)
            {
                return;
            }

            AnchorShape = range[range.Count];
        }
    }
}
