using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSprakIDE.Logging
{
    using static ConsoleColor;

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

        private static readonly ColorScheme traceColors = new ColorScheme(Red, DarkGray);

        private readonly IWriter _writer;
        private LogEvent _entry;
        private string _lastDate;
        private ColorScheme _currentColors;

        private class RecentMemory
        {
            public string Text;
            public Exception Exception;
            public object MarkID;
            public int Count;
        }

        private Queue<RecentMemory> _recentMemories = new Queue<RecentMemory>();

        public Output(IWriter writer)
        {
            _writer = writer;
        }

        public void Emit(LogEvent entry)
        {
            foreach (RecentMemory memory in _recentMemories)
            {
                if (entry.Text == memory.Text && entry.Exception == memory.Exception)
                {
                    memory.Count += 1;
                    _writer.Edit(memory.MarkID, $" (x{memory.Count})");
                    return;
                }
            }

            _entry = entry;
            _currentColors = colorLookup[entry.Level];

            _writer.Color = DarkGray;
            LogFormatUtility.WritePrefix(_writer, _entry, ref _lastDate);

            if (!colorLookup.TryGetValue(_entry.Level, out ColorScheme colors))
                colors = new ColorScheme(Magenta, Gray);

            WriteHighlightedText(entry.Text, colors);

            _writer.Color = colors.Dark;
            _writer.Write("");
            object lineEndMark = _writer.Mark();

            _writer.WriteLine();

            if (_entry.Exception != null)
                WriteException(_entry.Exception);

            _recentMemories.Enqueue(new RecentMemory {
                Text = _entry.Text,
                Exception = _entry.Exception,
                MarkID = lineEndMark,
                Count = 1
            });

            if (_recentMemories.Count > 3)
            {
                RecentMemory old = _recentMemories.Dequeue();
                _writer.ClearMark(old.MarkID);
            }
        }

        private void WriteHighlightedText(string text, ColorScheme colors)
        {
            // Darken text in brackets a bit - a minimal level
            // of highlighting.

            _currentColors = colors;

            const string start = @"\(";
            const string end = @"\)";
            int currentIndex = 0;

            while (true)
            {
                Match match = Regex.Match(text[currentIndex..], start);
                if (!match.Success)
                    break;

                WriteLight(text[currentIndex..match.Index]);
                currentIndex = match.Index;

                match = Regex.Match(text[currentIndex..], end);
                if (!match.Success)
                    break;

                WriteDark(text[currentIndex..(match.Index + 1)]);
                currentIndex = match.Index + 1;
            }

            WriteLight(text[currentIndex..]);
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
