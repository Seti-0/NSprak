﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

using NSprakIDE.Controls.Output;
using NSprakIDE.Controls.General;

namespace NSprakIDE.Controls
{
    public class OutputLogSupplier : ViewSupplier<OutputLog>
    {
        public OutputLogSupplier(ViewSelect view) : base(view) {}

        public OutputLog Start(string id, string name, string category)
        {
            OutputLog log = new OutputLog(name, category);

            Start(log, id, name, category);
            return log;
        }
    }

    public partial class OutputView : UserControl
    {
        private static readonly FlowDocument Empty = new FlowDocument();

        public OutputLogSupplier Supplier { get; }

        public OutputView()
        {
            InitializeComponent();
            Supplier = new OutputLogSupplier(ViewSelect);
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            OutputLog selection = (OutputLog)e.NewValue;
            RichText.Document = selection?.Document ?? Empty;
            RichText.ScrollToEnd();
        }
    }
}
