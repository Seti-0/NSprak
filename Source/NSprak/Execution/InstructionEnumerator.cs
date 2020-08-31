using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NSprak.Exceptions;
using NSprak.Operations;
using NSprak.Operations.Types;

namespace NSprak.Execution
{
    public class InstructionEnumerator
    {
        private IReadOnlyList<Op> _operations;
        private IReadOnlyList<OpDebugInfo> _debugInfo;
        private IReadOnlyDictionary<string, int> _labels;

        public bool SkipComments { get; set; } = true;

        public int Index { get; private set; }

        public int PreviousIndex { get; private set; }

        public bool HasNext => Index < _operations.Count - 1;

        public bool HasCurrent => Index >= 0 && Index < _operations.Count;

        public bool HasPrevious => PreviousIndex >= 0 && PreviousIndex < _operations.Count;

        public Op Current => _operations[Index];

        public OpDebugInfo CurrentInfo => _debugInfo[Index];

        public Op Previous => _operations[PreviousIndex];

        public OpDebugInfo PreviousInfo => _debugInfo[PreviousIndex];

        public void Reset(Executable executable)
        {
            PreviousIndex = -1;
            Index = -1;

            _operations = executable.Instructions;
            _debugInfo = executable.DebugInfo;
            _labels = executable.Labels;
        }

        public bool Step()
        {
            if (Index >= _operations.Count)
                return false;

            PreviousIndex = Index;
            Index++;

            if (SkipComments)
            {
                while (HasCurrent && Current is Pass)
                    Index++;
            }

            return HasCurrent;
        }

        public void EnsureStarted()
        {
            if (Index == -1)
                Step();
        }

        public void Jump(int newIndex)
        {
            if (newIndex > _operations.Count)
                throw new SprakInternalExecutionException(
                    $"Tried to jump to instruction {newIndex}, but instruction count is {_operations.Count}");

            PreviousIndex = Index;
            Index = newIndex;
        }

        public void Jump(string label)
        {
            if (!_labels.TryGetValue(label, out int newIndex))
                throw new SprakInternalExecutionException($"Label \"{label}\" not found");

            if (newIndex > _operations.Count)
                throw new SprakInternalExecutionException($"Label \"{label}\" refers to an instruction index that is out of range");

            PreviousIndex = Index;
            Index = newIndex;
        }
    }
}
