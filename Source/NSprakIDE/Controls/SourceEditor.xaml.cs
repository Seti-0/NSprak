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

namespace NSprakIDE.Controls
{
    /// <summary>
    /// Interaction logic for SourceEditor.xaml
    /// </summary>
    public partial class SourceEditor : UserControl
    {
        private DelayHelper _editAwaitor = new DelayHelper();

        private TokenColorizer _tokenColorizer;
        private ExpressionColorizer _expressionColorizer;
        private DiffMargin _diffMargin;
        private string _originalSource;

        private RuntimeHighlighter _runtimeHighlighter;

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

        public int CaretOffset
        {
            get => MainEditor.CaretOffset;
        }

        public Executor Executor
        {
            get => _runtimeHighlighter.Executor;
            set => _runtimeHighlighter.Executor = value;
        }

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

            MainEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromRgb(160, 200, 241));
            MainEditor.TextArea.SelectionBorder = null;
            MainEditor.TextArea.SelectionCornerRadius = 0;
            MainEditor.TextArea.SelectionForeground = null;

            _editAwaitor.Complete += OnFinishedEditing;
            MainEditor.TextChanged += OnMainEditorTextChanged;

            MainEditor.Options.ShowColumnRuler = true;

            _diffMargin = new DiffMargin();
            MainEditor.TextArea.LeftMargins.Add(_diffMargin);

            LineNumberMargin margin = new LineNumberMargin()
            {
                TextView = MainEditor.TextArea.TextView,
                Margin = new Thickness(5, 0, 20, 0),
            };
            Brush marginBrush = (Brush)FindResource("NSprakIDE.Toolbar");
            margin.SetValue(ForegroundProperty, marginBrush);
            MainEditor.TextArea.LeftMargins.Add(margin);
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
