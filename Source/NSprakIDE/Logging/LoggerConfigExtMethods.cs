using System;
using System.Collections.Generic;
using System.Text;

using Serilog;
using Serilog.Configuration;

namespace NSprakIDE.Logging
{
    public static class LoggerConfigExtMethods
    {
        public static LoggerConfiguration File(this LoggerSinkConfiguration config, 
            string path, IFormatProvider provider = null)
        {
            return config.Sink(new SimpleOutput(new FileWriter(path), provider));
        }

        public static LoggerConfiguration Debug(this LoggerSinkConfiguration config,
            IFormatProvider provider = null)
        {
            return config.Sink(new SimpleOutput(new DebugWriter(), provider));
        }
    }
}
