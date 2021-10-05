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

        public SignatureResolver SignatureResolver { get; }

        public Memory Memory { get; } 

        public InstructionEnumerator Instructions { get; }

        public bool ExitRequested { get; private set; }

        public ExecutionContext(Computer computer, SignatureResolver resolver)
        {
            Computer = computer;
            SignatureResolver = resolver;
            Instructions = new InstructionEnumerator();

            FrameDebugInfo mainFrame 
                = new FrameDebugInfo(Instructions, FunctionSignature.Main);
            Memory = new Memory(new SprakConverter(resolver, this), mainFrame);

            Reset();
        }

        public void Reset()
        {
            Executable = Computer.Executable;

            ExitRequested = false;
            Instructions.Reset(Executable);

            Memory.Reset();
        }

        public void BeginFrame(int index, FunctionSignature debugSignature)
        {
            Memory.FrameDebugInfo.Peek().FixLocation();

            ExecutionScope debugScope = Memory.CurrentScope;
            FrameDebugInfo debugInfo = new FrameDebugInfo(Instructions, debugSignature, debugScope);

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
            Memory.FrameDebugInfo.Peek().ClearFixedLocation();
        }

        public void RequestExit()
        {
            ExitRequested = true;
        }
    }
}
