using NSprak.Expressions.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NSprakIDE.Logging
{
    public class FileWriter : ISimpleWriter
    {
        private string _filePath;

        public FileWriter(string filePath)
        {
            _filePath = filePath;
        }

        public void Write(string text)
        {
            File.AppendAllText(_filePath, text);
        }

        public void WriteLine(string text = "")
        {
            File.AppendAllText(_filePath, text + "\n");
        }
    }
}
