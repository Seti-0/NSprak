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

namespace NSprakIDE.Controls
{
    public class MessageWrapper
    {
        public string Severity { get; }

        public string UserMessage { get; }

        public string Line { get; }

        public string Column { get; }

        public MessageWrapper(Message)
        {
            Name = name;
            Value = value?.ToString();
            SprakType = value?.Type?.InternalName;
        }
    }

    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
        }
    }
}
