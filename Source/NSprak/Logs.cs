using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace NSprak
{
    public static class Logs
    {
        public static ILogger Core { get; private set; } = new DummyLogger();

        public static void Init(ILogger logger)
        {
            Core = logger;
        }

        private class DummyLogger : ILogger, IDisposable
        {
            public IDisposable BeginScope<TState>(TState state)
                => this;

            public void Dispose() { }

            public bool IsEnabled(LogLevel logLevel)
                => false;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter)
            { }
        }
    }
}
