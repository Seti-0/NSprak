using System;

namespace NSprakIDE.Logging
{
    public interface IWriter : ISimpleWriter
    {
        public ConsoleColor Color { set; get; }

        public object Mark();

        public bool ClearMark(object id);

        public bool Edit(object id, string newText);
    }
}
