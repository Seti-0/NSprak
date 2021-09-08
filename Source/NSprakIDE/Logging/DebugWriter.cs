using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NSprakIDE.Logging
{
    public class DebugWriter : ISimpleWriter
    {
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
