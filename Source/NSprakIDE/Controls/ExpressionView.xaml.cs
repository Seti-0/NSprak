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
        private NSprakBlock _root;
        private bool _showDebugInfo = false;

        public bool ShowDebug
        {
            get => _showDebugInfo;

            set
            {
                if (value != _showDebugInfo)
                {
                    _showDebugInfo = value;
                    Update();
                    UpdateToggleButtonText();
                }
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
            UpdateToggleButtonText();
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

        private void UpdateToggleButtonText()
        {
            if (ShowDebug)
                ShowDebugButton.Content = "Hide Debug Info";
            else
                ShowDebugButton.Content = "Show Debug Info";
        }
    }
}
