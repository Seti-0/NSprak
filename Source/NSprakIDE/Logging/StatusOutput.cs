using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace NSprakIDE.Logging
{
    public class StatusOutput :ILogEventSink
    {
        private readonly ISimpleWriter _writer;

        public StatusOutput(ISimpleWriter writer)
        {
            _writer = writer;
        }

        public void Emit(LogEvent entry)
        {
            string[] lines = entry.Text.Split('\n');

            string statusMessage = lines[0];
            if (entry.Exception != null)
                statusMessage += " (See log view for error trace)";

            _writer.WriteLine(statusMessage);
        }
    }
}
