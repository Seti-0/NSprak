using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NSprakIDE.Logging
{
    public enum LogLevel
    {
        Debug, Information, Warning, Error
    }

    public interface ILogEventSink
    {
        void Emit(LogEvent entry);
    }

    public class LogEvent
    {
        public DateTime Timestamp { get; }

        public LogLevel Level { get; }

        public string Text { get; }

        public Exception Exception { get; }

        public LogEvent(LogLevel level, string text, Exception exception)
        {
            Timestamp = DateTime.UtcNow;
            Level = level;
            Text = text;
            Exception = exception;
        }
    }

    public class Log
    {
        private readonly ConcurrentQueue<ILogEventSink> _sinks
            = new ConcurrentQueue<ILogEventSink>();

        private readonly ConcurrentQueue<LogEvent> _events 
            = new ConcurrentQueue<LogEvent>();

        public void AddSink(ILogEventSink sink)
        {
            foreach (LogEvent previousEvent in _events)
                sink.Emit(previousEvent);

            _sinks.Enqueue(sink);
        }

        public void LogDebug(string text)
            => AddEntry(new LogEvent(LogLevel.Information, text, null));

        public void LogInformation(string text)
            => AddEntry(new LogEvent(LogLevel.Information, text, null));

        public void LogWarning(string text)
            => AddEntry(new LogEvent(LogLevel.Information, text, null));

        public void LogError(string text, Exception e)
            => AddEntry(new LogEvent(LogLevel.Information, text, e));

        public void AddEntry(LogEvent entry) {
            AddEntry(entry, _sinks);
        }

        private void AddEntry(LogEvent entry, IEnumerable<ILogEventSink> targets)
        {
            _events.Enqueue(entry);

            List<ILogEventSink> successful = new List<ILogEventSink>();
            List<LogEvent> failEvents = new List<LogEvent>();

            foreach (ILogEventSink target in targets)
            {
                try
                {
                    target.Emit(entry);
                    successful.Add(target);
                }
                catch (Exception e)
                {
                    LogEvent failEvent = new LogEvent(
                        LogLevel.Error, "Logging exception", e);

                    failEvents.Add(failEvent);
                }
            }

            if (failEvents.Count > 0)
            {
                if (successful.Count == 0)
                    throw new Exception(
                        "Exception while logging",
                        failEvents.First().Exception
                    );

                else foreach (LogEvent failEvent in failEvents)
                    AddEntry(failEvent, successful);
            }
        }
    }
}
