using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace NSprakIDE.Logging
{
    public interface IWriter
    {
        public void Begin();

        public void End();

        public void Write(string text);

        public void WriteLine(string text = "");
    }

    public class DirectOutput : ILogOutput
    {
        private IWriter _writer;
        private string _lastDate;
        private LogEntry _entry;

        public DirectOutput(IWriter writer)
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

        private void ApplyIndent()
        {
            LogFormatUtility.ApplyIndent(_writer, _entry.Indent);
        }

        public void Send(LogEntry entry)
        {
            _entry = entry;

            LogFormatUtility.ApplyIndent(_writer, entry.Indent);
            LogFormatUtility.WritePrefix(_writer, entry, ref _lastDate);
            _writer.WriteLine(entry.Message);

            if (entry.Exception != null)
                WriteException(entry.Exception);

            _entry = null;
        }

        private void WriteException(Exception e)
        {
            if (e == null)
                return;

            string name = e.GetType().Name;
            string message = e.Message;

            ApplyIndent();
            _writer.WriteLine($"[{name}] {message}");

            IEnumerable<StackFrame> trace = new StackTrace(e, true).GetFrames();
            WriteStackTrace(trace);

            if (e.InnerException != null)
            {
                ApplyIndent();
                _writer.WriteLine("Caused by Inner Exception:");
                WriteException(e.InnerException);
            }
        }


        private void WriteStackTrace(IEnumerable<StackFrame> trace)
        {
            if (trace == null)
            {
                ApplyIndent();
                _writer.WriteLine("(Stacktrace is null)");
                return;
            }

            bool missingInfo = false;

            foreach (var frame in trace)
            {
                LogFormatUtility.ApplyIndent(_writer, _entry.Indent + 1);
                LogFormatUtility.GetSignatureElements(frame.GetMethod(), out string methodName, out string arguments);

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
