using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

using NSprak.Messaging;

namespace NSprakIDE.Controls
{
    public class MessageWrapper
    {
        public string Severity { get; }

        public string UserMessage { get; }

        public string Line { get; }

        public string Column { get; }

        public MessageWrapper(Message message)
        {
            Severity = message.Template.Severity.ToString();
            UserMessage = message.RenderedText;

            if (message.Location != null)
            {
                Line = message.Location.LineStart.ToString();
                Column = message.Location.Start.ToString();
            }
            else
            {
                Line = "";
                Column = "";
            }
        }
    }

    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public Messenger Target { get; set; }

        public MessageView()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            MessagesGrid.ItemsSource = null;
            MessagesGrid.Items.Clear();
        }

        public void Update()
        {
            if (Target == null)
            {
                Clear();
                return;
            }
            else
            {
                List<MessageWrapper> messages = Target.Messages
                    .Select(x => new MessageWrapper(x))
                    .ToList();

                MessagesGrid.ItemsSource = messages;
            }
        }
    }
}
