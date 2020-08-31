using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace NSprakIDE.Commands
{
    using static CommandHelper;

    public static class GeneralCommands
    {
        public readonly static ICommand

                Rename = CreateCommand("Rename", Key.F2),
                RefreshView = CreateCommand("Refresh View", Key.F5);
    }
}
