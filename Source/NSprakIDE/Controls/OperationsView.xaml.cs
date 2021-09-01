using NSprak.Execution;
using NSprak.Operations.Types;
using NSprak.Language;
using NSprakIDE.Controls.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NSprak.Operations;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace NSprakIDE.Controls
{
    using static Operations.OpBrushNames;

    /// <summary>
    /// Interaction logic for OperationsView.xaml
    /// </summary>
    public partial class OperationsView : UserControl
    {
        private class InstructionLine
        {
            public static Brush BreakpointBackground;
            public static Brush ActiveBackground;

            private Brush _defaultBackground;

            public IEnumerable<Run> Runs { get; }
            public bool Breakpoint { get; set; }
            public bool Active { get; set; }

            public InstructionLine(IEnumerable<Run> runs, Brush defaultBackground)
            {
                Runs = runs;

                _defaultBackground = defaultBackground;

                UpdateBackground();
            }

            public void UpdateBackground()
            {
                Brush brush;

                if (Active) brush = ActiveBackground;
                else if (Breakpoint) brush = BreakpointBackground;
                else brush = _defaultBackground;

                foreach (Run run in Runs)
                    run.Background = brush;
            }
        }

        private Executable _target;

        private FlowDocument _document;
        private Paragraph _paragraph;
        private int _currentLineLength;

        private List<InstructionLine> _lines = new List<InstructionLine>();
        private List<Run> _currentLine;
        private InstructionLine _currentHighlighted;
        private bool _alternateLineColors;

        private double _previousFocus;

        private int _indexWidth;

        public Executable Target
        {
            get => _target;

            set
            {
                _target = value;
                Update();
            }
        }

        public OperationsView()
        {
            InitializeComponent();

            InstructionLine.ActiveBackground = (Brush)FindResource(Next);
            InstructionLine.BreakpointBackground = (Brush)FindResource(Breakpoint);
        }

        private void RefreshVisual()
        {
            MainText.Document = _document;
            MainText.InvalidateVisual();
        }

        public void ClearHighlight()
        {
            if (_currentHighlighted != null)
            {
                _currentHighlighted.Active = false;
                _currentHighlighted.UpdateBackground();

                _currentHighlighted = null;
                _previousFocus = 0;

                RefreshVisual();
            }
        }

        public void Highlight(int index)
        {
            // The field is cleared by "ClearHighlight", but we will need it in
            // this function
            double previousFocus = _previousFocus;

            ClearHighlight();

            if (_lines.Count == 0)
                return;

            if (index >= _lines.Count)
                index = _lines.Count - 1;

            Brush brush = (Brush)FindResource(Next);
            _lines[index].Active = true;
            _lines[index].UpdateBackground();

            _currentHighlighted = _lines[index];

            EnsureLineInView(_lines[index], previousFocus);

            RefreshVisual();
        }

        public void ToggleBreakpoint()
        {
            TextPointer pointer = MainText.CaretPosition;
            int index = GetIndex(pointer);

            if (index < 0 || index >= _target.InstructionCount)
                return;

            InstructionLine line = _lines[index];
            OpDebugInfo info = _target.DebugInfo[index];

            info.Breakpoint = !info.Breakpoint;
            line.Breakpoint = info.Breakpoint;
            line.UpdateBackground();
        }

        private void EnsureLineInView(InstructionLine line, double previousFocus)
        {
            // The coordinates here confused me greatly. I suspect it's
            // because all three of "pointer.GetRect", "CaretPosition.GetRect"
            // and "ScrollTo" are different coordinate systems? 

            // I don't see how to check this without going to the WPF source

            // But it seems to work, and I now I am reluctant to touch it

            // The goal is to scroll such that the line is visible.
            //
            //   -  In any case, make sure the line has a margin around it
            //   -  If there is no previous focus, consider the top of the document to be the previous focus
            //
            //   -  If the previous focus is close enough, don't scroll at all
            //   -  If the previous focus is further but still close enough, scroll just enough
            //   -  Else, scroll so that the line is at the top

            TextPointer pointer = line
                    .Runs
                    .FirstOrDefault()
                    ?.ContentStart;

            double target = pointer.GetCharacterRect(LogicalDirection.Forward).Bottom;
            double current = MainText.CaretPosition.GetCharacterRect(LogicalDirection.Forward).Top;

            // This coordinate transform makes little sense to me, but it seems to work 
            target = target - current;
            
            double previousTarget = previousFocus;

            // This is the height of the scroll window, not the entire document!
            double viewHeight = MainText.ActualHeight;

            double targetMargin = 40;
            double previousMargin = 20;

            // Figure out if the previous target is close enough for us to consider it
            double gap = previousMargin + Math.Abs(target - previousTarget) + targetMargin;
            bool minimizeJump = gap < viewHeight;

            double newHeight;
            double jump;
            bool move;

            if (minimizeJump)
            {
                bool forwards = target > previousTarget;

                if (forwards)
                {
                    // This is the minimum new height
                    newHeight = target + targetMargin - viewHeight;

                    jump = current + newHeight;
                    // But don't move unless we need to move forwards to get there
                    move = jump > 0;
                }
                else
                {
                    // This is the maximum new height
                    newHeight = target - targetMargin;

                    // But don't move unless we need to move backwards to get there
                    jump = current + newHeight;
                    move = jump < 0;
                }
            }
            else
            {
                // The previous target is too far away, don't consider it
                // and put the line at the top
                newHeight = target - targetMargin;
                move = true;
            }

            if(move)
                MainText.ScrollToVerticalOffset(newHeight);

            _previousFocus = target;
        }

        public void Update()
        {
            MainText.Document.Blocks.Clear();

            if (_target == null)
                return;

            NewDocument();

            int n = _target.Instructions.Count;
            _indexWidth = (int)Math.Log10(n) + 1;
            
            for (int i = 0; i < n; i++)
            {
                Op op = _target.Instructions[i];
                OpDebugInfo info = _target.DebugInfo[i];
                Write(op, info, i);
            }

            RefreshVisual();
        }

        private void StartInstructionLine()
        {
            _currentLine = new List<Run>();
        }

        private void SaveInstructionLine()
        {
            bool alternate = _alternateLineColors && (_lines.Count % 2) == 1;
            string key = alternate ? OpBackgroundAlternate : OpBackground;
            Brush background = (Brush)FindResource(key);

            _lines.Add(new InstructionLine(_currentLine, background));
        }

        private void NewDocument()
        {
            _document = new FlowDocument();
            _lines.Clear();
            NewParagraph();
        }

        private void NewParagraph()
        {
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);
            _currentLineLength = 0;
        }

        private void LineBreak()
        {
            _paragraph.Inlines.Add(new LineBreak());
            _currentLineLength = 0;
        }

        private void Space()
        {
            Write(Main, " ");
            _currentLineLength += 1;
        }

        private void PadLeft(string colorKey, string text, int totalLength)
        {
            if (_currentLineLength + text.Length < totalLength)
                text = text.PadLeft(totalLength - text.Length - _currentLineLength);

            Write(colorKey, text);
        }

        private void Write(string foregroundKey, string text, bool save = true)
        {
            Run run = new Run(text);

            if (foregroundKey != null)
            {
                Brush foreground = (Brush)FindResource(foregroundKey);
                run.Foreground = foreground;
            }

            _paragraph.Inlines.Add(run);
            _currentLineLength += text.Length;

            if (save)
                _currentLine.Add(run);
        }

        private void WriteIndex(int index, bool hide)
        {
            string text = index.ToString().PadLeft(_indexWidth);
            text = text.PadRight(_indexWidth + 3);

            // If hide is true, draw the text, but as the same color as the background
            // See GetIndex for the horrible reason for this silliness
            string coloKey = hide ? OpBackground : IndexForeground;

            Write(IndexForeground, text, save: false);
        }

        private int GetIndex(TextPointer pointer)
        {
            // This is a fantastically inefficient and delicate and silly way
            // of determining the instruction index, but line indexes and flow documents
            // are messy to begin with.

            // Ultimately, the way to fix this is to replace the rich text box with an avalon text editor
            
            TextPointer lineStart = pointer.GetLineStartPosition(0) ?? pointer.DocumentStart;
            TextPointer nextLineStart = pointer.GetLineStartPosition(1) ?? pointer.DocumentEnd;

            string text = new TextRange(lineStart, nextLineStart).Text;

            Match match = Regex.Match(text, @"\d+");

            if (!match.Success)
                return -1;

            if (int.TryParse(match.Value, out int guess))
                return guess;

            return -1;
        }

        private void Write(Op op, OpDebugInfo info, int index)
        {
            if (op is Pass)
            {
                _alternateLineColors = false;

                NewParagraph();

                StartInstructionLine();

                WriteIndex(index, hide: true);
                Write(Comment, " # "+ info.Comment + " ");
                
                SaveInstructionLine();

                LineBreak();
                NewParagraph();
                return;
            }

            _alternateLineColors = true;

            StartInstructionLine();

            WriteIndex(index, hide: false);
            Write(Main, " " + op.Name);

            object rawParam = op.RawParam;

            if (rawParam != null)
            {
                Space();

                PadLeft(Comment, "", 42);

                string colorKey = op.RawParam switch
                {
                    Value _ => ValueParam,
                    int _ => NumberParam,
                    string _ => NameParam,
                    _ => Param
                };

                Write(colorKey, op.RawParam.ToString());
            }

            //PadLeft(Comment, "", 70);

            if (info.Comment != null)
                Write(Comment, info.Comment);

            Write(Main, " ");

            SaveInstructionLine();

            LineBreak();
        }
    }
}
