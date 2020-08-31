using System;
using System.Collections.Generic;
using System.Text;
using NSprak.Language.Builtins;
using NSprak.Operations;

namespace NSprak.Execution
{
    public class Executable
    {
        public Dictionary<FunctionSignature, int> EntryPoints { get; }

        public IReadOnlyList<Op> Instructions { get; }

        public IReadOnlyList<OpDebugInfo> DebugInfo { get; }

        public IReadOnlyDictionary<string, int> Labels { get; }

        public int InstructionCount => Instructions.Count;

        public Executable()
        {
            Instructions = new Op[0];
            DebugInfo = new OpDebugInfo[0];
            Labels = new Dictionary<string, int>();
            EntryPoints = new Dictionary<FunctionSignature, int>();
        }

        public Executable(List<Op> ops, List<OpDebugInfo> debugInfo,
            Dictionary<FunctionSignature, int> entryPoints, Dictionary<string, int> labels)
        {
            Instructions = ops;
            DebugInfo = debugInfo;
            Labels = labels;

            EntryPoints = entryPoints;
        }
    }
}
