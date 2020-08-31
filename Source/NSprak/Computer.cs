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

        public MessageCollection Messages { get; } = new MessageCollection();

        public Compiler Compiler { get; }

        public Executable Executable { get; private set; } = new Executable();

        public IConsole StandardOut { get; set; }

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

        public void Compile()
        {
            CompilationEnvironment environment = new CompilationEnvironment(
                Messages, _signatureLookup, _assignmentLookup);

            Executable = Compiler.Compile(Source, environment);
        }

        public Executor CreateExecutor()
        {
            return new Executor(this, _signatureLookup);
        }
    }
}
