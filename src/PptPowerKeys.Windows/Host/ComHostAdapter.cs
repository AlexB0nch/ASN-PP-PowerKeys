using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.PowerPoint;
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
