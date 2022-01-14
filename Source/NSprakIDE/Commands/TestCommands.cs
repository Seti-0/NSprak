using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Input;

namespace NSprakIDE.Commands
{
    using static CommandHelper;

    public class TestCommands
    {
        public readonly static ICommand
            Find = CreateCommand("Find Tests", Key.F5),
            Run = CreateCommand("Run Tests", Key.F6);
    }
}
