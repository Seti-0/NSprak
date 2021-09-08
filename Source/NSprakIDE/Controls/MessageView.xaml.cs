using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

using NSprak.Messaging;

using NSprakIDE.Controls.General;

namespace NSprakIDE.Controls
{
    public class MessageWrapper
    {
        public string Severity { get; }

        public string UserMessage { get; }

        public string Line { get; }

        public string Column { get; }

        public string Source { get; }

        public MessageWrapper(Message message, string source)
        {
            Severity = message.Template.Severity.ToString();
            UserMessage = message.RenderedText;

            if (message.Location != null)
            {
                Line = (message.Location.LineStart + 1).ToString();
                Column = (message.Location.ColumnStart + 1).ToString();
            }
            else
            {
                Line = "";
                Column = "";
            }

            Source = source;
        }
    }

    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public Messenger Target { get; set; }

        public ViewSupplier<Messenger> Supplier { get; }

        public MessageView()
        {
            InitializeComponent();

            Supplier = new ViewSupplier<Messenger>(ViewSelect);
            Supplier.AllowNoSelection = true;
        }

        public void Clear()
        {
            MessagesGrid.ItemsSource = null;
            MessagesGrid.Items.Clear();
        }

        public void Update()
        {
            IViewItem selectedItem = ViewSelect.SelectedItem;
            if (selectedItem == null)
            {
                Clear();
                return;
            }

            Messenger selectedMessenger = (Messenger)selectedItem.GetValue();

            List<MessageWrapper> messages = new List<MessageWrapper>();
            if (selectedMessenger == null)
            {
                foreach (ViewItem<Messenger> supplierItem in Supplier.ViewItems)
                    if (supplierItem.Value != null)
                        messages.AddRange(supplierItem.Value.Messages.Select(
                            x => new MessageWrapper(x, supplierItem.Name)));
            }
            else
                messages.AddRange(selectedMessenger.Messages.Select(
                    x => new MessageWrapper(x, selectedItem.Name)));

            MessagesGrid.ItemsSource = messages;
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            Update();
        }
    }
}
