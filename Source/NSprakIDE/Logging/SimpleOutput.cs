using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace NSprakIDE.Logging
{
    public interface IWriter
    {
        public void Write(string text);

        public void WriteLine(string text = "");
    }

    public class SimpleOutput :ILogEventSink
    {
        private IWriter _writer;
        private string _lastDate;

        public SimpleOutput(IWriter writer)
        {
            _writer = writer;
        }

        public void Emit(LogEvent entry)
        {
            string message = entry.Text;

            LogFormatUtility.WritePrefix(_writer, entry, ref _lastDate);
            _writer.WriteLine(message);

            if (entry.Exception != null)
                WriteException(entry.Exception);
        }

        private void WriteException(Exception e)
        {
            if (e == null)
                return;

            string name = e.GetType().Name;
            string message = e.Message;

            _writer.WriteLine($"[{name}] {message}");

            IEnumerable<StackFrame> trace = new StackTrace(e, true).GetFrames();
            WriteStackTrace(trace);

            if (e.InnerException != null)
            {
                _writer.WriteLine("Caused by Inner Exception:");
                WriteException(e.InnerException);
            }
        }


        private void WriteStackTrace(IEnumerable<StackFrame> trace)
        {
            if (trace == null)
            {
                _writer.WriteLine("(Stacktrace is null)");
                return;
            }

            bool missingInfo = false;

            foreach (var frame in trace)
            {
                LogFormatUtility.GetSignatureElements(
                    frame.GetMethod(), out string methodName, out string arguments);

                string fileName = frame.GetFileName();
                bool missingFilename = string.IsNullOrWhiteSpace(fileName);
                missingInfo |= missingFilename;

                int lineNumber = frame.GetFileLineNumber();
                bool missingLineNumber = lineNumber == 0;
                missingInfo |= missingLineNumber;

                _writer.Write($"at ");

                if (!missingLineNumber)
                    _writer.Write($"line {lineNumber} of ");

                _writer.Write($"{methodName}{arguments}");

                if (!missingFilename)
                    _writer.Write($"in {frame.GetFileName()}");

                _writer.WriteLine();
            }

            if (missingInfo)
                _writer.WriteLine($"(Some info is missing. This is expected if release-mode code is involved)");
        }
    }
}
