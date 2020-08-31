using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NSprakIDE.Logging
{
    public class DebugWriter : IWriter
    {
        public void Begin()
        {
            Debug.Write("Begin Log: " + DateTime.Now);
        }

        public void End()
        {
            Debug.Write("End of Log");
        }

        public void Write(string text)
        {
            Debug.Write(text);
        }

        public void WriteLine(string text = "")
        {
            Debug.WriteLine(text);
        }
    }
}
