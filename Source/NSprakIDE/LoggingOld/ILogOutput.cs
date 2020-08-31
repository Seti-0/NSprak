using System;
using System.Collections.Generic;
using System.Text;

namespace NSprakIDE.Logging
{
    public interface ILogOutput
    {
        public void Begin();

        public void End();

        public void Send(LogEntry entry);
    }
}
