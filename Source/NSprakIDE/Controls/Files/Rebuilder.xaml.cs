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

namespace NSprakIDE.Controls.Files
{
    public interface IRebuildRequester
    {
        public event EventHandler RebuildRequested;
    }

    /// <summary>
    /// Interaction logic for Rebuilder.xaml
    /// </summary>
    public partial class Rebuilder : UserControl
    {
        public Func<IRebuildRequester> BuildAction { get; }

        public Rebuilder(Func<IRebuildRequester> buildAction)
        {
            InitializeComponent();

            BuildAction = buildAction;
            Rebuild();
        }

        public void Rebuild()
        {
            IRebuildRequester content = BuildAction();
            content.RebuildRequested += OnRebuildRequested;
            Main.Content = content;
        }

        private void OnRebuildRequested(object obj, EventArgs e)
        {
            Rebuild();
        }
    }
}
