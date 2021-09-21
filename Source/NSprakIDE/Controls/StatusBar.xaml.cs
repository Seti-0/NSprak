using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Output;
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
    public partial class StatusBar : UserControl
    {
        private OutputLog _source;

        private readonly DelayHelper _delay 
            = new DelayHelper(TimeSpan.FromSeconds(2));

        public OutputLog Source
        {
            get => _source;

            set
            {
                if (value != _source)
                {
                    if (_source != null)
                        _source.ItemAdded -= _source_ItemAdded;

                    _source = value;

                    if (_source != null)
                        _source.ItemAdded += _source_ItemAdded;
                }
            }
        }

        public StatusBar()
        {
            InitializeComponent();

            void Clear() => TextView.Text = "";

            _delay.Complete += (obj, e) => 
                TextView.Dispatcher.Invoke(Clear);
        }

        private void _source_ItemAdded(object sender, OutputLogEvent e)
        {
            string item = e.Text.Split("\n")[0];

            void Action()
            {
                TextView.Text = item;
                _delay.Poke();
            }

            TextView.Dispatcher.Invoke(Action);
        }
    }
}
