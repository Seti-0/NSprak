using System;
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

        protected override OutputLog Create(string name, string category)
        {
            return new OutputLog(name, category, this);
        }
    }

    public partial class OutputView : UserControl
    {
        public OutputLogSupplier Supplier { get; }

        public OutputView()
        {
            InitializeComponent();
            Supplier = new OutputLogSupplier(ViewSelect);
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            OutputLog selection = (OutputLog)e.NewValue;
            RichText.Document = selection.Document;
            RichText.ScrollToEnd();
        }
    }
}
