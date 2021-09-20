using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NSprak;
using NSprak.Tokens;
using NSprak.Messaging;
using NSprak.Execution;

using NSprakIDE.Controls.Source;
using NSprakIDE.Controls.General;

using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;

namespace NSprakIDE.Controls
{
    /// <summary>
    /// Interaction logic for SourceEditor.xaml
    /// </summary>
    public partial class SourceEditor : UserControl
    {
        private readonly DelayHelper _editAwaitor = new DelayHelper();

        private readonly TokenColorizer _tokenColorizer;
        private readonly ExpressionColorizer _expressionColorizer;
        private readonly DiffMargin _diffMargin;
        private string _originalSource;

        private readonly RuntimeHighlighter _runtimeHighlighter;

        public event EventHandler<EventArgs> FinishedEditing;

        public string Text
        {
            get => MainEditor.Text;

            set
            {
                MainEditor.Text = value;
                _originalSource = value;
            }
        }

        public bool HasChanges => _diffMargin.HasChanges;

        public int CaretOffset
        {
            get => MainEditor.CaretOffset;
        }

        public Executor Executor
        {
            get => _runtimeHighlighter.Executor;
            set => _runtimeHighlighter.Executor = value;
        }

        public event EventHandler<EventArgs> HasChangesChanged;

        public SourceEditor(Messenger messenger)
        {
            InitializeComponent();

            _tokenColorizer = new TokenColorizer()
            {
                Elements = new List<IColorizerElement<Token>>
                {
                    new SyntaxHighlighting(),
                    new ErrorHighlighter(messenger)
                }
            };

            _runtimeHighlighter = new RuntimeHighlighter();
            _expressionColorizer = new ExpressionColorizer()
            {
                Elements = new List<IColorizerElement<NSprak.Expressions.Expression>>
                {
                    //new TestExpressions(),
                    _runtimeHighlighter
                }
            };

            MainEditor.TextArea.TextView.LineTransformers.Add(_tokenColorizer);
            MainEditor.TextArea.TextView.LineTransformers.Add(_expressionColorizer);

            MainEditor.TextArea.SelectionBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 200, 241));
            MainEditor.TextArea.SelectionBorder = null;
            MainEditor.TextArea.SelectionCornerRadius = 0;
            MainEditor.TextArea.SelectionForeground = null;

            _editAwaitor.Complete += OnFinishedEditing;
            MainEditor.TextChanged += OnMainEditorTextChanged;

            MainEditor.Options.ShowColumnRuler = true;

            _diffMargin = new DiffMargin();
            _diffMargin.HasChangesChanged += DiffMargin_HasChangesChanged;
            MainEditor.TextArea.LeftMargins.Add(_diffMargin);

            LineNumberMargin margin = new LineNumberMargin()
            {
                TextView = MainEditor.TextArea.TextView,
                Margin = new Thickness(5, 0, 20, 0),
            };
            Brush marginBrush = (Brush)FindResource("NSprakIDE.Toolbar");
            margin.SetValue(ForegroundProperty, marginBrush);
            MainEditor.TextArea.LeftMargins.Add(margin);

            //MainEditor.TextArea.TextEntering += TextArea_TextEntering;
            //MainEditor.TextArea.TextEntered += TextArea_TextEntered;
        }

        /*
        CompletionWindow _window;

        public class Data : ICompletionData
        {
            public ImageSource Image => null;

            public string Text { get; }

            public object Content => Text + " (Content)";

            public object Description => new TextBlock(new Run("Hello World\nHello!"));

            public double Priority => 32.4;

            public Data(string text)
            {
                Text = text;
            }

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                Logs.Core.LogInformation("Complete: " + Text);
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                _window = new CompletionWindow(MainEditor.TextArea);
                _window.WindowStyle = WindowStyle.None;
                _window.ResizeMode = ResizeMode.NoResize;
                IList<ICompletionData> data = _window
                    .CompletionList.CompletionData;

                data.Add(new Data("Item 1"));
                data.Add(new Data("Item 2"));
                data.Add(new Data("Item 3"));

                _window.Show();
                _window.Closed += (obj, e) =>
                {
                    _window = null;
                };
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _window != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    _window.CompletionList.RequestInsertion(e);
                };
            }
        }
        */

        private void DiffMargin_HasChangesChanged(object sender, EventArgs e)
        {
            // This is silly, and the diff/change tracker itself should really 
            // be made separate from the margin.
            OnHasChangesChanged();
        }

        protected virtual void OnHasChangesChanged()
        {
            HasChangesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void EnsureLineIsVisible(int lineNumber, int columnNumber)
        {
            MainEditor.ScrollTo(lineNumber, columnNumber);
        }

        public void Update(Compiler compiler)
        {
            _tokenColorizer.Tokens = compiler.Tokens;
            _expressionColorizer.Tree = compiler.ExpressionTree;
        }

        public void ResetDiff()
        {
            _originalSource = Text;
            _diffMargin.Update(Text, Text);
        }

        public void Redraw()
        {
            MainEditor.TextArea.TextView.Redraw();
        }

        private void OnMainEditorTextChanged(object sender, EventArgs e)
        {
            _editAwaitor.Poke();
        }

        private void OnFinishedEditing(object sender, EventArgs e)
        {
            FinishedEditing?.Invoke(this, e);
            _diffMargin.Dispatcher.Invoke(() => 
                _diffMargin.Update(Text, _originalSource));
        }
    }
}
