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
using System.Windows.Shapes;

namespace NSprakIDE.Controls.General
{
    public enum DialogType
    {
        YesNo, YesNoCancel
    }

    public partial class Dialog : Window
    {
        public DialogType Type { get; }

        public Dialog(string title, string message, DialogType type)
        {
            InitializeComponent();

            Title = title;
            Message.Text = message;

            Type = type;
            if (type != DialogType.YesNoCancel)
                Cancel.Visibility = Visibility.Collapsed;
        }

        private void OnYes(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnNo(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            Close();
        }
    }
}
