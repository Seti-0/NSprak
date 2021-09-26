using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Output;
using NSprakIDE.Logging;
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
    public partial class StatusBar : UserControl, ISimpleWriter
    {
        private readonly DelayHelper _delay 
            = new DelayHelper(TimeSpan.FromSeconds(2));

        private bool _startNewLine = false; 

        public StatusBar()
        {
            InitializeComponent();

            void Clear() => TextView.Text = "";

            _delay.Complete += (obj, e) => 
                TextView.Dispatcher.Invoke(Clear);
        }

        public void Write(string text)
        {
            Write(text, false);
        }

        public void WriteLine(string text = "")
        {
            Write(text, true);
        }

        private void Write(string text, bool end)
        {
            void Action()
            {
                if (_startNewLine)
                    TextView.Text = "";

                TextView.Text += text;

                _startNewLine = end;
                _delay.Poke();
            }

            Dispatcher.Invoke(Action);
        }
    }
}
