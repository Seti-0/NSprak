using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NSprakIDE.Controls.Expressions
{
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
