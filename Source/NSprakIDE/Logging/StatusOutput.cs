using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;

namespace NSprakIDE.Logging
{
    public class StatusOutput :ILogEventSink
    {
        private readonly ISimpleWriter _writer;
        private readonly MainWindow _main;

        public StatusOutput(ISimpleWriter writer, MainWindow main)
        {
            _writer = writer;
            _main = main;
        }

        public void Emit(LogEvent entry)
        {
            string[] lines = entry.Text.Split('\n');

            string statusMessage = lines[0];
            
            if (entry.Exception != null)
                statusMessage += " (See log view for error trace)";

            _writer.WriteLine(statusMessage);

            if (entry.Exception != null)
                _main.ShowLogView();
        }
    }
}
