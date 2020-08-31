using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NSprakIDE.Logging
{
    using static ConsoleColor;

    public interface IColoredWriter : IWriter
    {
        public ConsoleColor Color { set; get; }
    }

    public class DirectColoredOutput : ILogOutput
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

        private static readonly Dictionary<LogType, ColorScheme> colorLookup = new Dictionary<LogType, ColorScheme>
        {
            { LogType.Error, new ColorScheme(Red, DarkRed) },
            { LogType.Important, new ColorScheme(Blue, DarkBlue) },
            { LogType.Info, new ColorScheme(Gray, DarkGray) },
            { LogType.Success, new ColorScheme(Green, DarkGreen) },
            { LogType.Warning, new ColorScheme(Yellow, DarkYellow) }
        };

        private static readonly ColorScheme traceColors = new ColorScheme(DarkRed, DarkGray);

        private IColoredWriter _writer;
        private LogEntry _entry;
        private string _lastDate;
        private string _previousText;
        private ColorScheme _currentColors;

        public DirectColoredOutput(IColoredWriter writer)
        {
            _writer = writer;
        }

        public void Begin()
        {
            _writer.Begin();
        }

        public void End()
        {
            _writer.End();
        }

        public void Send(LogEntry entry)
        {
            _entry = entry;
            _currentColors = colorLookup[entry.Type];

            LogFormatUtility.ApplyIndent(_writer, entry.Indent);

            _writer.Color = DarkGray;
            LogFormatUtility.WritePrefix(_writer, _entry, ref _lastDate);

            // The idea is to apply a little bit of shading - words that 
            // appeared in the previous message will be darkened.

            if (!colorLookup.TryGetValue(_entry.Type, out ColorScheme colors))
                colors = new ColorScheme(Magenta, Gray);

            const string pattern = @"[^\w]+";
            string remainingText = _entry.Message;
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

            _previousText = _entry.Message;

            if (_entry.Exception != null)
                WriteException(_entry.Exception);
        }

        private void ApplyIndent(int offset = 0)
        {
            LogFormatUtility.ApplyIndent(_writer, _entry.Indent + offset);
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

            ApplyIndent();
            WriteLight($"[{name}] {message}");
            _writer.WriteLine();

            IEnumerable<StackFrame> trace = new StackTrace(e, true).GetFrames();
            WriteStackTrace(trace);

            if (e.InnerException != null)
            {
                ApplyIndent();
                WriteDark("Caused by Inner Exception:");
                WriteException(e.InnerException);
            }
        }

        private void WriteStackTrace(IEnumerable<StackFrame> trace)
        {
            if (trace == null)
            {
                ApplyIndent();
                WriteDark("(Stacktrace is null)");
                return;
            }

            bool missingInfo = false;

            foreach (var frame in trace)
            {
                ApplyIndent(offset: 1);

                LogFormatUtility.SplitMethodSignature(frame.GetMethod().ToString(), out string methodName, out string arguments);

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
                    WriteLight(" of ");
                }

                WriteLight(methodName);
                WriteDark(arguments);

                if (!missingFilename)
                    WriteDark($" in {frame.GetFileName()}");

                _writer.WriteLine();
            }

            if (missingInfo)
                WriteDark($"(Some info is missing. This is expected if release-mode code is involved)");
        }
    }
}
