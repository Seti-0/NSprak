using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace NSprakIDE.Logging.Sinks
{
    public class OutputSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            MessageTemplateToken token = logEvent.MessageTemplate.Tokens.First();

            if (token is PropertyToken prop)
            {
                
            }
        }
    }
}
