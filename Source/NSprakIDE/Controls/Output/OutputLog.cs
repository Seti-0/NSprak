using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace NSprakIDE.Controls.Output
{
    public class OutputLog
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public Color Color { get; set; }

        public FlowDocument Document { get; } = new FlowDocument();

        private OutputLogSupplier _parent;
        private Paragraph _paragraph;

        public OutputLog(string name, string category, OutputLogSupplier parent)
        {
            Name = name;
            Category = category;
            _parent = parent;

            _paragraph = new Paragraph();
            Document.Blocks.Add(_paragraph);
        }

        public void End()
        {
            _parent.End(this);
        }

        public void Write(string text)
        {
            Document.Dispatcher.Invoke(() => WriteUnsafe(text));
        }

        private void WriteUnsafe(string text)
        {
            Run run = new Run(text);
            run.Foreground = new SolidColorBrush(Color);

            _paragraph.Inlines.Add(run);
            //_parent.UpdateSelection();

            if (_paragraph.Inlines.Count > 100)
                _paragraph.Inlines.Remove(_paragraph.Inlines.FirstInline);
        }

        public void WriteLine(string text)
        {
            Write(text + "\n");
        }
    }
}
