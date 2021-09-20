using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Functions;
using NSprak.Functions.Resolution;
using NSprak.Messaging;

namespace NSprak
{
    public class Computer
    {
        private readonly SignatureResolver _signatureLookup;
        private readonly AssignmentResolver _assignmentLookup;

        // This should not be visible. Actually, the resolvers shouldn't
        // be fields of the computer at all. This is to be revisited at some 
        // point.
        public SignatureResolver Resolver => _signatureLookup;

        public Compiler Compiler { get; }

        public Executable Executable { get; private set; } = new Executable();

        public IComputerScreen Screen { get; set; }

        public Messenger Messenger { get; set; } = new Messenger();

        public string Source { get; set; }

        public List<Library> Libraries = new List<Library>
        {
            Library.Core
        };

        public Computer()
        {
            Compiler = new Compiler();

            _assignmentLookup = new AssignmentResolver(Libraries);
            _signatureLookup = new SignatureResolver(Libraries, _assignmentLookup);

            _signatureLookup.SpecifyOperationBindings(new List<OperationBinding>());
        }

        public bool Compile()
        {
            Messenger.Clear();

            CompilationEnvironment environment = new CompilationEnvironment(
                Messenger, _signatureLookup, _assignmentLookup);

            Executable exe = Compiler.Compile(Source, environment);
            
            if (exe == null)
                return false;

            else
            {
                Executable = exe;
                return true;
            }
        }

        public Executor CreateExecutor()
        {
            return new Executor(this, _signatureLookup);
        }
    }
}
