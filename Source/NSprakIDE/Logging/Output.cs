using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NSprakIDE.Logging
{
    using static ConsoleColor;

    public interface IColoredWriter : IWriter
    {
        public ConsoleColor Color { set; get; }
    }

    public class Output : ILogEventSink
    {
        private class ColorScheme
        {
            public ConsoleColor Light;
            public ConsoleColor Dark;

            public ColorScheme(ConsoleColor light, ConsoleColor dark)
            {
                Light = light;
                Dark = dark;
            }
        }

        private static readonly Dictionary<LogLevel, ColorScheme> colorLookup 
            = new Dictionary<LogLevel, ColorScheme>
        {
            { LogLevel.Error, new ColorScheme(Red, DarkRed) },
            { LogLevel.Warning, new ColorScheme(Yellow, DarkYellow) },
            { LogLevel.Information, new ColorScheme(Gray, DarkGray) },
            { LogLevel.Debug, new ColorScheme(Gray, DarkGray) },
        };

        private static readonly ColorScheme traceColors = new ColorScheme(DarkRed, DarkGray);

        private IColoredWriter _writer;
        private LogEvent _entry;
        private string _lastDate;
        private string _previousText;
        private ColorScheme _currentColors;

        public Output(IColoredWriter writer, IFormatProvider formatProvider = null)
        {
            _writer = writer;
        }

        public void Emit(LogEvent entry)
        {
            _entry = entry;
            _currentColors = colorLookup[entry.Level];

            _writer.Color = DarkGray;
            LogFormatUtility.WritePrefix(_writer, _entry, ref _lastDate);

            // The idea is to apply a little bit of shading - words that 
            // appeared in the previous message will be darkened.

            if (!colorLookup.TryGetValue(_entry.Level, out ColorScheme colors))
                colors = new ColorScheme(Magenta, Gray);

            string message = entry.Text;

            const string pattern = @"[^\w]+";
            string remainingText = message;
            bool isRepeat;

            _previousText ??= "";

            while (LogFormatUtility.Split(remainingText, pattern, out string upto, out string after))
            {
                isRepeat = _previousText.Contains(upto);
                _writer.Color = isRepeat ? colors.Dark : colors.Light;
                _writer.Write(upto);
                remainingText = after;
            }

            isRepeat = _previousText.Contains(remainingText);
            _writer.Color = isRepeat ? colors.Dark : colors.Light;
            _writer.WriteLine(remainingText);

            _previousText = message;

            if (_entry.Exception != null)
                WriteException(_entry.Exception);
        }

        private void WriteLight(string text)
        {
            _writer.Color = _currentColors.Light;
            _writer.Write(text);
        }

        private void WriteDark(string text)
        {
            _writer.Color = _currentColors.Dark;
            _writer.Write(text);
        }

        private void WriteException(Exception e)
        {
            if (e == null)
                return;

            string name = e.GetType().Name;
            string message = e.Message;

            _currentColors = traceColors;

            WriteLight($"[{name}] {message}");
            _writer.WriteLine();

            IEnumerable<StackFrame> trace = new StackTrace(e, true).GetFrames();
            WriteStackTrace(trace);

            if (e.InnerException != null)
            {
                WriteDark("Caused by Inner Exception:");
                _writer.WriteLine();
                WriteException(e.InnerException);
            }
        }

        private void WriteStackTrace(IEnumerable<StackFrame> trace)
        {
            if (trace == null)
            {
                WriteDark("(Stacktrace is null)");
                return;
            }

            bool missingInfo = false;

            foreach (var frame in trace)
            {
                LogFormatUtility.GetSignatureElements(frame.GetMethod(), out string methodName, out string arguments);

                string fileName = frame.GetFileName();
                bool missingFilename = string.IsNullOrWhiteSpace(fileName);
                missingInfo |= missingFilename;

                int lineNumber = frame.GetFileLineNumber();
                bool missingLineNumber = lineNumber == 0;
                missingInfo |= missingLineNumber;

                WriteDark($"at ");

                if (!missingLineNumber)
                {
                    WriteDark("line ");
                    WriteLight(lineNumber.ToString());
                    WriteDark(" in ");
                }

                WriteLight(methodName);
                WriteDark(arguments);

                if (!missingFilename)
                    WriteDark($" in {frame.GetFileName()}");

                _writer.WriteLine();
            }

            if (missingInfo)
            {
                WriteDark($"(Some info is missing. This is expected if release-mode code is involved)");
                _writer.WriteLine();
            }
        }
    }
}
