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

namespace NSprakIDE.Controls.Expressions
{
    using NSprakExpression = NSprak.Expressions.Expression;

    /// <summary>
    /// Interaction logic for ElementRenderer.xaml
    /// </summary>
    public partial class ElementRenderer : UserControl
    {
        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(
                        "Element", typeof(VisualElement), typeof(ElementRenderer), new PropertyMetadata(OnExpressionChanged));

        protected static void OnExpressionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as ElementRenderer).UpdateText();
        }

        public VisualElement Element
        {
            get => GetValue(ElementProperty) as VisualElement;

            set
            {
                SetValue(ElementProperty, value);
                UpdateText();
            }
        }

        public ElementRenderer()
        {
            InitializeComponent();

        }

        public void UpdateText()
        {
            Main.SelectAll();
            Main.Selection.Text = "";

            Element.RenderTo(Main);
        }
    }
}
