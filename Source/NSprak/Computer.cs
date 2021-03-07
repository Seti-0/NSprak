using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Execution;
using NSprak.Language;
using NSprak.Messaging;

namespace NSprak
{
    public class Computer
    {
        private SignatureResolver _signatureLookup;
        private AssignmentResolver _assignmentLookup;

        public Compiler Compiler { get; }

        public Executable Executable { get; private set; } = new Executable();

        public IConsole StandardOut { get; set; }

        public IMessenger Messenger { get; set; }

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
