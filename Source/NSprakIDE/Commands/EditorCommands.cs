using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Input;

namespace NSprakIDE.Commands
{
    using static CommandHelper;

    public class EditorCommands
    {
        public readonly static ICommand

           // File

            Save = CreateCommand("Save", Key.S, ModifierKeys.Control),

            // View

            ViewCode = CreateCommand("ViewCode", "Code"),
            ViewExpressionTree = CreateCommand("ViewExpressionTree", "Expressions"),
            ViewOperations = CreateCommand("ViewOperations", "Operations"),

            // Debugging

            Start = CreateCommand("Start", "Run",  Key.F5, ModifierKeys.Control),
            StartDebug = CreateCommand("StartDebug", "Start", Key.F5),
            Stop = CreateCommand("Stop", Key.F5, ModifierKeys.Control),
            Restart = CreateCommand("Restart"),

            Pause = CreateCommand("Break", Key.Pause, ModifierKeys.Control & ModifierKeys.Alt),

            StepOver = CreateCommand("StepOver", "Step over", Key.F10),
            StepInto = CreateCommand("StepInto", "Step into", Key.F11),
            StepOut = CreateCommand("StepOutOf", "Step out", Key.F11, ModifierKeys.Shift),

            // Breakpoints

            ToggleBreakpoint = CreateCommand("ToggleBreakpoint", "Toggle Breakpoint", Key.F9);
    }
}
