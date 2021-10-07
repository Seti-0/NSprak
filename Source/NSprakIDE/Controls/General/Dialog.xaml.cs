using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NSprakIDE.Controls.General
{
    public enum DialogType
    {
        YesNo, YesNoCancel
    }

    // I know there is already a DialogResult as part of the Window,
    // but I could only ever get that to be true/false, and never with 
    // no value (for cancel).
    public enum DialogResponse
    {
        Yes, No, Cancel
    }

    public partial class Dialog : Window
    {
        public DialogType Type { get; }

        public DialogResponse Response { get; private set; }

        public Dialog(string title, string message, DialogType type)
        {
            InitializeComponent();

            Title = title;
            Message.Text = message;

            Type = type;
            if (type != DialogType.YesNoCancel)
                Cancel.Visibility = Visibility.Collapsed;

            Response = DialogResponse.Cancel;
        }

        private void OnYes(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Response = DialogResponse.Yes;
            Close();
        }

        private void OnNo(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Response = DialogResponse.No;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            Response = DialogResponse.Cancel;
            Close();
        }
    }
}
