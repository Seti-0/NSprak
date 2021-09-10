using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Patterns;
using NSprak.Expressions.Structure;
using NSprak.Expressions.Structure.Transforms;
using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions
{
    public class ExpressionTree
    {
        private List<ITreeTransform> _transforms = new List<ITreeTransform>
        {
            new UpdateParentHints(),
            new CollectDeclarations(),
            new ResolveTypesAndSignatures()
        };

        private List<Expression> _flatStatements = new List<Expression>();

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

            Root = TreeBuilder.Build(_flatStatements, environment);

            foreach (ITreeTransform transform in _transforms)
                transform.Apply(Root, environment);
        }
    }
}
