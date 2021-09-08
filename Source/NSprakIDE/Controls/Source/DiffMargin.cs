using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

using NSprakIDE.Themes;

namespace NSprakIDE.Controls.Source
{
    public enum DiffPathAction
    {
        Insert, Remove, Pass
    }

    public enum DiffMarginKind
    {
        Start, Pass, Insert, Change
    }

    public class DiffMarginElement
    {
        public DiffMarginKind Kind { get; }

        public int RemoveCount { get; set; }

        public DiffMarginElement(DiffMarginKind kind)
        {
            Kind = kind;
        }
    }

    public class DiffMargin : AbstractMargin
    {

        public const double DefaultWidth = 4;

        public List<DiffMarginElement> _margin = new List<DiffMarginElement>();

        public bool HasChanges { get; private set; }

        public event EventHandler<EventArgs> HasChangesChanged;

        protected virtual void OnHasChangesChanged()
        {
            HasChangesChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
                oldTextView.VisualLinesChanged -= TextView_VisualLinesChanged;
            
            if (newTextView != null)
                newTextView.VisualLinesChanged += TextView_VisualLinesChanged;
        }

        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        public void Update(string current, string original)
        {
            List<DiffPathAction> diffPath = Diff(
                current.Split("\n"),
                original.Split("\n"),
                out bool hasChanges
            );

            List<DiffMarginElement> margin = GetMargin(diffPath);

            Dispatcher.Invoke(() =>
            {
                _margin = margin;

                if (hasChanges != HasChanges)
                {
                    HasChanges = hasChanges;
                    OnHasChangesChanged();
                }

                InvalidateVisual();
            });
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Math.Min(DefaultWidth, availableSize.Width), 0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (TextView == null || !TextView.VisualLinesValid)
                return;

            Brush insertBrush = Theme.GetBrush(Theme.Source.DiffInsert);
            Brush removeBrush = Theme.GetBrush(Theme.Source.DiffRemove);
            Brush changeBrush = Theme.GetBrush(Theme.Source.DiffChange);

            IEnumerable<(VisualLine, int)> visualLines = TextView
                .VisualLines
                .Select(x => (x, x.FirstDocumentLine.LineNumber));

            List<(int number, double y, double height)> lines 
                = new List<(int, double, double)>();

            foreach (VisualLine line in TextView.VisualLines)
            {
                int number = line.FirstDocumentLine.LineNumber;
                if (number >= _margin.Count)
                    continue;

                double y = line.GetTextLineVisualYPosition(
                    line.TextLines[0], VisualYPosition.LineTop);
                y -= TextView.VerticalOffset;

                double height = line.Height;

                lines.Add((number, y, height));
            }

            double width = RenderSize.Width;

            foreach ((int number, double y, double height) in lines)
            {
                Brush brush;
                switch (_margin[number].Kind)
                {
                    case DiffMarginKind.Insert: brush = insertBrush; break;
                    case DiffMarginKind.Change: brush = changeBrush; break;
                    default: continue;
                }

                if (brush != null)
                {
                    Rect rect = new Rect(0, y, width, height);
                    drawingContext.DrawRectangle(brush, null, rect);
                }
            }

            foreach ((int number, double y, double height) in lines)
            {
                if (_margin[number].RemoveCount > 0)
                {
                    // I'd like to think that there is an easier way to
                    // draw a triangle than all this, but I do not see one.
                    Point top = new Point(0, y + 0.8*height);
                    Point middle = new Point(width, y + height);
                    Point bottom = new Point(0, y + 1.2 * height);

                    PathFigure figure = new PathFigure(top,
                        new PathSegment[] { 
                            new LineSegment(middle, true),
                            new LineSegment(bottom, true)
                        },
                        closed: true
                    );

                    PathGeometry geometry = new PathGeometry();
                    geometry.Figures.Add(figure);

                    Pen pen = new Pen(removeBrush, 2);
                    drawingContext.DrawGeometry(removeBrush, pen, geometry);
                }
            }
        }

        private List<DiffMarginElement> GetMargin(List<DiffPathAction> diffPath)
        {
            List<DiffMarginElement> margin = new List<DiffMarginElement>();
            margin.Add(new DiffMarginElement(DiffMarginKind.Start));

            // The most recent element with removals is being tracked
            // for the sake of converting insertions to changes.
            DiffMarginElement latestWithRemove = null;

            foreach (DiffPathAction action in diffPath)
            {
                if (action == DiffPathAction.Pass)
                {
                    margin.Add(new DiffMarginElement(DiffMarginKind.Pass));
                    latestWithRemove = null;
                }

                else if (action == DiffPathAction.Remove)
                {
                    margin.Last().RemoveCount += 1;
                    latestWithRemove = margin.Last();
                }

                else
                {
                    DiffMarginKind kind;

                    if (latestWithRemove != null)
                    {
                        latestWithRemove.RemoveCount -= 1;
                        kind = DiffMarginKind.Change;

                        if (latestWithRemove.RemoveCount == 0)
                            latestWithRemove = null;
                    }
                    else
                    {
                        kind = DiffMarginKind.Insert;
                        latestWithRemove = null;
                    }

                    margin.Add(new DiffMarginElement(kind));
                }
            }

            return margin;
        }

        private List<DiffPathAction> Diff<T>(IList<T> after, IList<T> before, 
            out bool changes)
        {

            /*
            General difference algorithm based on
            "An O(ND) Difference Algorithm and Its Variations"
            by Eugene Myers

            The implementation has been adapted from Myers to favor
            placing removals before insertions rather than after them, but
            is otherwise much the same.

            Note to self: a Python notebook was used to figure this one out,
            see the Diff notebook in the  'misc' repo for a detailed explanation.

            Finally, I'm using dictionaries for the ease of negative indexing,
            but that does seem very silly performance and readability wise
            so I might come back to that.
            */

            int N = after.Count;
            int M = before.Count;

            int D = 0;
            int k = 0;
            
            Dictionary<int, int> positions = new Dictionary<int, int>();
            positions[1] = -1;

            Dictionary<int, Dictionary<int, List<DiffPathAction>>> steps 
                = new Dictionary<int, Dictionary<int, List<DiffPathAction>>>();

            // Search

            bool found = false;

            while (D <= M + N)
            {
                steps[D] = new Dictionary<int, List<DiffPathAction>>();
                k = -D;

                while (k <= D)
                {
                    steps[D][k] = new List<DiffPathAction>();
                    int y;
                    
                    if (k == -D)
                    {
                        y = positions[k + 1] + 1;
                        steps[D][k].Add(DiffPathAction.Remove);
                    }
                    else if (k == D)
                    {
                        y = positions[k - 1];
                        steps[D][k].Add(DiffPathAction.Insert);
                    }
                    else
                    {
                        int w1 = 2 * positions[k + 1] + 1;
                        int w2 = 2 * positions[k - 1] - 1;

                        if (w1 > w2)
                        {
                            y = positions[k + 1] + 1;
                            steps[D][k].Add(DiffPathAction.Remove);
                        }
                        else
                        {
                            y = positions[k - 1];
                            steps[D][k].Add(DiffPathAction.Insert);
                        }
                    }

                    int x = y + k;

                    while (x < N && y < M 
                        && EqualityComparer<T>.Default.Equals(after[x], before[y]))
                    {
                        x += 1;
                        y += 1;
                        steps[D][k].Add(DiffPathAction.Pass);
                    }

                    positions[k] = y;

                    if (x >= N && y >= M)
                    {
                        found = true;
                        break;
                    }

                    k += 2;
                }

                if (found == true)
                    break;

                D += 1;
            }

            // Backtrace

            changes = D > 0;

            List<IEnumerable<DiffPathAction>> elements 
                = new List<IEnumerable<DiffPathAction>>();

            while (D > 0)
            {
                elements.Add(steps[D][k]);

                if (steps[D][k][0] == DiffPathAction.Insert)
                    k -= 1;
                else
                    k += 1;

                D -= 1;
            }

            elements.Add(steps[0][k].Skip(1));

            elements.Reverse();

            List<DiffPathAction> result = new List<DiffPathAction>();
            foreach (IEnumerable<DiffPathAction> element in elements)
                result.AddRange(element);
 
            return result;
        }
    }
}
