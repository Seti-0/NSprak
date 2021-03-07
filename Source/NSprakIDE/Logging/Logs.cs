using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Extensions.Logging;

using Serilog;

using NSprakIDE.Logging;

using MicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using SerilogLogger = Serilog.ILogger;

namespace NSprakIDE
{
    public class Core { }

    public static class Logs
    {
        public static ILoggerFactory Factory { get; }

        public static MicrosoftLogger Core { get; private set; }

        static Logs()
        {
            SerilogLogger logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.File("log.txt")
                .MinimumLevel.Debug()
                .CreateLogger();

            Factory = new LoggerFactory();
            Factory.AddSerilog(logger);

            string name = $"{nameof(NSprakIDE)}.{nameof(Core)}";
            Core = Factory.CreateLogger(name);

            name = $"{nameof(NSprak)}.{nameof(Core)}";
            NSprak.Logs.Init(Factory.CreateLogger(name));
        }
    }
}
