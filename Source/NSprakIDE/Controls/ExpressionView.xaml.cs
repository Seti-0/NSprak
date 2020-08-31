using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using NSprakIDE.Controls.Expressions;
using NSprak.Expressions.Types;

namespace NSprakIDE.Controls
{
    using NSprakBlock = Block;

    /// <summary>
    /// Interaction logic for ExpressionView.xaml
    /// </summary>
    public partial class ExpressionView : UserControl
    {
        private static readonly DependencyProperty ShowDebugProperty
            = DependencyProperty.Register("ShowDebug", typeof(bool), typeof(ExpressionView), new PropertyMetadata(OnShowDebugChanged));

        private static void OnShowDebugChanged(DependencyObject changed, DependencyPropertyChangedEventArgs args)
        {
            ((ExpressionView)changed).Update();
        }

        private NSprakBlock _root;

        public bool ShowDebug
        {
            get => (bool) GetValue(ShowDebugProperty);

            set
            {
                SetValue(ShowDebugProperty, value);
                Update();
            }
        }

        public NSprakBlock Root
        {
            get => _root;

            set
            {
                _root = value;
                Update();
            }
        }

        public ExpressionView()
        {
            InitializeComponent();
        }

        private void Update()
        {
            VisualElement wrapper = new VisualElement(Root, ShowDebug);
            MainTree.ItemsSource = new VisualElement[] { wrapper };
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDebug = !ShowDebug;
        }
    }
}
