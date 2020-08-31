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
using NSprakIDE.Controls.Code;
using NSprakIDE.Controls.General;

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

        public event EventHandler<EventArgs> FinishedEditing;

        public string Text
        {
            get => MainEditor.Text;

            set
            {
                MainEditor.Text = value;
            }
        }

        public SourceEditor()
        {
            InitializeComponent();

            _tokenColorizer = new TokenColorizer()
            {
                Elements = new List<IColorizerElement<Token>>
                {
                    new SyntaxHighlighting(new SyntaxHighlighterTheme(TryFindBrush))
                }
            };

            _expressionColorizer = new ExpressionColorizer()
            {
                Elements = new List<IColorizerElement<NSprak.Expressions.Expression>>
                {
                    //new TestExpressions()
                }
            };

            MainEditor.TextArea.TextView.LineTransformers.Add(_tokenColorizer);
            MainEditor.TextArea.TextView.LineTransformers.Add(_expressionColorizer);

            _editAwaitor.Complete += OnFinishedEditing;
            MainEditor.TextChanged += OnMainEditorTextChanged;
        }

        private Brush TryFindBrush(string key)
        {
            return TryFindResource(key) as Brush;
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
        }
    }
}
