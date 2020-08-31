using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace NSprakIDE.Commands
{
    using static CommandHelper;

    public static class FileCommands
    {
        public readonly static ICommand

            OpenSelected = CreateCommand("Open Selected", "Open", Key.Enter),
            OpenInFileExplorer = CreateCommand("Open in File Explorer"),

            AddFile = CreateCommand("Add File"),
            AddFolder = CreateCommand("Add Folder");
    }
}
