using System;
using System.Collections.Generic;
using System.Text;
using NSprak.Exceptions;
using NSprak.Functions;
using NSprak.Functions.Resolution;
using NSprak.Functions.Signatures;
using NSprak.Language;

namespace NSprak.Execution
{
    public class ExecutionContext
    {
        public Computer Computer { get; }

        public Executable Executable { get; private set; }

        public Memory Memory { get; } 

        public InstructionEnumerator Instructions { get; } = new InstructionEnumerator();

        public bool ExitRequested { get; private set; }

        public ExecutionContext(Computer computer, SignatureResolver resolver)
        {
            Computer = computer;
            Memory = new Memory(new SprakConverter(resolver, this));
        }

        public void Reset()
        {
            Executable = Computer.Executable;

            ExitRequested = false;
            Memory.Reset();
            Instructions.Reset(Executable);
        }

        public void BeginFrame(int index, FunctionSignature debugInfo)
        {
            Memory.Frames.Push(Instructions.Index + 1);
            Instructions.Jump(index);

            Memory.FrameDebugInfo.Push(debugInfo);
        }

        public void Return()
        {
            if (Memory.Frames.Count == 0)
                throw new SprakInternalExecutionException("No frames to return out of");

            Instructions.Jump(Memory.Frames.Pop());
            Memory.FrameDebugInfo.Pop();
        }

        public void RequestExit()
        {
            ExitRequested = true;
        }
    }
}
