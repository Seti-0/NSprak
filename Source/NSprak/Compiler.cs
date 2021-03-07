using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Execution;
using NSprak.Expressions;
using NSprak.Language;
using NSprak.Messaging;
using NSprak.Operations;
using NSprak.Tokens;

namespace NSprak
{
    public class CompilationEnvironment
    {
        public MessageCollection Messages { get; }

        public SignatureResolver SignatureLookup { get; }

        public AssignmentResolver AssignmentLookup { get; }

        public CompilationEnvironment(MessageCollection messages, SignatureResolver signatureLookup, AssignmentResolver assignmentLookup)
        {
            Messages = messages;
            SignatureLookup = signatureLookup;
            AssignmentLookup = assignmentLookup;
        }
    }

    public class Compiler
    {
        public bool IsCompiled { get; }

        public bool IsLatest { get; }

        public TokenPage Tokens { get; } = new TokenPage();

        public ExpressionTree ExpressionTree { get; } = new ExpressionTree();

        public Executable Compile(string source, CompilationEnvironment environment)
        {
            Tokens.Update(source, environment.Messages);
            ExpressionTree.Update(Tokens, environment);

            if (!environment.Messages.HasErrors)
                return CodeGenerator.Create(ExpressionTree);

            else return null;
        }
    }
}
