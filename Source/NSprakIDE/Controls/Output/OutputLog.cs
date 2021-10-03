using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace NSprakIDE.Controls.Output
{
    public class OutputLogEvent : EventArgs
    {
        public string Text { get; }

        public OutputLogEvent(string text)
        {
            Text = text;
        }
    }

    public class OutputLog
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public Color Color { get; set; }

        public FlowDocument Document { get; } = new FlowDocument();

        private readonly Paragraph _paragraph;

        private readonly Dictionary<int, Run> _markers 
            = new Dictionary<int, Run>();
        private int _markerID;

        private string _currentItem;

        public event EventHandler<OutputLogEvent> ItemAdded;

        public OutputLog(string name, string category)
        {
            Name = name;
            Category = category;

            _paragraph = new Paragraph();
            Document.Blocks.Add(_paragraph);
        }

        public void Write(string text)
        {
            _currentItem += text;
            Document.Dispatcher.Invoke(() => WriteUnsafe(text));
        }

        public void WriteLine(string text)
        {
            Write(text + "\n");

            OnItemAdded(new OutputLogEvent(_currentItem));
            _currentItem = "";
        }

        public int Mark(string text = "")
        {
            while (_markers.ContainsKey(_markerID))
            {
                _markerID += 1;

                // I know. I'm very silly sometimes.
                if (_markerID == int.MaxValue)
                    return -1;
            }

            void Action()
            {
                Run run = new Run(text);
                _markers.Add(_markerID, run);
                WriteUnsafe(run);
            };

            Document.Dispatcher.Invoke(new Action(Action));
            return _markerID;
        }

        public bool ClearMark(object id)
        {
            if (id is int number)
            {
                _markers.Remove(number);
                return true;
            }

            return false;
        }

        public bool Edit(object id, string newText)
        {
            if (id is int number && _markers.TryGetValue(number, out Run run))
            {
                Document.Dispatcher.BeginInvoke(new Action(() => {
                    run.Text = newText;
                }));
                return true;
            }

            return false;
        }

        protected virtual void OnItemAdded(OutputLogEvent e)
        {
            ItemAdded?.Invoke(this, e);
        }

        private void WriteUnsafe(string text)
        {
            if (text.Length == 0)
                return;

            WriteUnsafe(new Run(text));
        }

        private void WriteUnsafe(Run run)
        {
            run.Foreground = new SolidColorBrush(Color);

            _paragraph.Inlines.Add(run);
        }
    }
}
