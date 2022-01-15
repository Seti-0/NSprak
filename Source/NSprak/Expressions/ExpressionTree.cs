using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Patterns;
using NSprak.Expressions.Structure;
using NSprak.Expressions.Structure.Transforms;
using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Messaging;
using NSprak.Tests;

namespace NSprak.Expressions
{
    public class ExpressionTree
    {
        private readonly List<ITreeTransform> _transforms = new List<ITreeTransform>
        {
            new UpdateParentHints(),
            new CollectDeclarations(),
            new ResolveTypesAndSignatures()
        };

        private readonly List<Expression> _flatStatements = new List<Expression>();

        public Block Root { get; private set; }

        public void Update(TokenPage page, CompilationEnvironment environment)
        {
            _flatStatements.Clear();

            Pattern pattern = MainPattern.Instance;
            foreach (PageLine line in page)
            {
                pattern.TryMatch(line, 
                    environment.Messages, out Expression expression);

                if (expression != null)
                    _flatStatements.Add(expression);
            }

            ParseTestCommands(page, environment.Messages);

            Root = TreeBuilder.Build(_flatStatements, environment);

            foreach (ITreeTransform transform in _transforms)
                transform.Apply(Root, environment);
        }

        private void ParseTestCommands(TokenPage page, Messenger messages)
        {
            foreach (PageLine line in page)
                foreach (Token token in line)
                    if (token.Type == TokenType.Comment && token.Content.StartsWith("#!"))
                    {
                        TestCommand.FromToken(token, messages);
                    }
        }
    }
}
