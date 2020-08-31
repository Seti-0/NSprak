using Fluent.StyleSelectors;
using NSprak.Expressions.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NSprakIDE.Logging
{
    public class FileWriter : IWriter
    {
        private string _filePath;
        private int headerLength;

        public FileWriter(string filePath)
        {
            _filePath = filePath;
        }

        public void Begin()
        {
            string header = $"============ Begin Log: {DateTime.Now} ============\n";
            File.AppendAllText(_filePath, header);
            headerLength = header.Length;
        }

        public void End()
        {
            File.AppendAllText(_filePath, new string('=', headerLength-1) + "\n");
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
