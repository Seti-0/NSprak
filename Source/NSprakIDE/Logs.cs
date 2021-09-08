using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprakIDE.Logging;

namespace NSprakIDE
{
    public static class Logs
    {
        public static Log Core = new Log();

        static Logs()
        {
            AddSink(new SimpleOutput(new DebugWriter()));
            AddSink(new SimpleOutput(new FileWriter("log.txt")));
        }

        public static void AddSink(ILogEventSink sink)
        {
            Core.AddSink(sink);
        }
    }
}
