using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Execution;
using NSprak.Expressions;
using NSprak.Functions;
using NSprak.Functions.Resolution;
using NSprak.Messaging;
using NSprak.Operations;
using NSprak.Tokens;

namespace NSprak
{
    public class CompilationEnvironment
    {
        public Messenger Messages { get; }

        public SignatureResolver SignatureLookup { get; set; }

        public AssignmentResolver AssignmentLookup { get; set; }

        public CompilationEnvironment(Messenger messages, SignatureResolver signatureLookup, AssignmentResolver assignmentLookup)
        {
            Messages = messages;
            SignatureLookup = signatureLookup;
            AssignmentLookup = assignmentLookup;
        }
    }

    public class Compiler
    {
        private readonly SignatureResolver _signatureLookup;
        private readonly AssignmentResolver _assignmentLookup;

        public bool IsCompiled { get; }

        public bool IsLatest { get; }

        public TokenPage Tokens { get; } = new TokenPage();

        public ExpressionTree ExpressionTree { get; } = new ExpressionTree();

        public Compiler()
        {
            // At some point this should be configurable.
            List<Library> libraries = new List<Library>
            {
                Library.Core
            };

            _assignmentLookup = new AssignmentResolver(libraries);
            _signatureLookup = new SignatureResolver(libraries, _assignmentLookup);

            _signatureLookup.SpecifyOperationBindings(new List<OperationBinding>());
        }

        public Executable Compile(string source, Messenger messenger)
        {
            CompilationEnvironment env = new CompilationEnvironment(
                messenger, _signatureLookup, _assignmentLookup);

            Tokens.Update(source, messenger);
            ExpressionTree.Update(Tokens, env);

            if (!env.Messages.HasErrors)
                return CodeGenerator.Create(ExpressionTree, _signatureLookup.UserDeclarations);

            else return null;
        }
    }
}
