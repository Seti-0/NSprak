using NSprakIDE.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace NSprakIDE.Logging
{
    public enum LogType
    {
        Debug, Error, Important, Info, Success, Warning
    }

    public class LogEntry
    {
        public string Message;
        public LogType Type;
        public Exception Exception;
        public DateTime Time = DateTime.Now;
        public int Indent;
    }

    public class Log
    {
        public static Log Core = new Log("Core");

        public static List<ILogOutput> Outputs { get; } = new List<ILogOutput>()
        {
            new DirectOutput(new DebugWriter())
        };

        public static int Indent { get; private set; }

        public static void Begin()
        {
            List<ILogOutput> passed = new List<ILogOutput>(); 
            List<LogEntry> fails = new List<LogEntry>(Outputs.Count);

            foreach (ILogOutput output in Outputs)
            {
                try
                {
                    output.Begin();
                    passed.Add(output);
                }
                catch (Exception e)
                {
                    string message = $"Exception on Begin() from log output: {output.GetType().Name}";
                    fails.Add(Create(LogType.Error, message, e));
                }
            }

            if (fails.Count > 0)
            {
                if (passed.Count == 0)
                {
                    string message = "Logging Failure";
                    Exception e = fails.First().Exception;

                    throw new Exception(message, e);
                }
                else
                {
                    foreach (LogEntry fail in fails)
                        Core.Append(fail, passed);
                }
            }
        }

        public static void End()
        {
            foreach (ILogOutput output in Outputs)
                output.End();
        }

        public static void PushIndent()
        {
            Indent++;
        }

        public static void PopIndent()
        {
            if (Indent == 0)
                return;

            Indent--;
        }

        public string Name { get; }

        public Log(string name)
        {
            Name = name;
        }

        public void Append(LogEntry entry, List<ILogOutput> targets = null)
        {
            targets ??= Outputs;

            List<ILogOutput> passed = new List<ILogOutput>(targets.Count);
            List<LogEntry> fails = new List<LogEntry>(targets.Count);

            foreach (ILogOutput output in targets)
            {
                try
                {
                    output.Send(entry);
                    passed.Add(output);
                }
                catch (Exception e)
                {
                    string message = $"Exception from log output: {output.GetType().Name}";
                    fails.Add(Create(LogType.Error, message, e));
                }
            }

            if (fails.Count > 0)
            {
                if (passed.Count == 0)
                {
                    string message = "Logging Failure";
                    Exception e = fails.First().Exception;

                    throw new Exception(message, e);
                }
                else
                {
                    foreach (LogEntry failure in fails)
                        Core.Append(failure, passed);
                }
            }
        }

        private static LogEntry Create(LogType type, string text, Exception exception)
        {
            return new LogEntry
            {
                Exception = exception,
                Indent = Indent,
                Message = text,
                Time = DateTime.Now,
                Type = type
            };
        }

        public void Debug(string text, Exception e = null)
        {
            Append(Create(LogType.Debug, text, e));
        }

        public void Error(string text, Exception e = null)
        {
            Append(Create(LogType.Error, text, e));
        }

        public void Important(string text, Exception e = null)
        {
            Append(Create(LogType.Important, text, e));
        }

        public void Info(string text, Exception e = null)
        {
            Append(Create(LogType.Info, text, e));
        }

        public void Success(string text, Exception e = null)
        {
            Append(Create(LogType.Success, text, e));
        }

        public void Warning(string text, Exception e = null)
        {
            Append(Create(LogType.Warning, text, e));
        }
    }
}
