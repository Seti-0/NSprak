using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Tokens;
using NSprak.Expressions.Structure;

namespace NSprak.Expressions.Types
{
    public class Block : Expression
    {
        public Header Header { get; }

        public Expression EndStatement { get; }

        public IReadOnlyList<Expression> Statements { get; }

        public override Token StartToken { get; }

        public override Token EndToken { get; }

        public Scope ScopeHint { get; set; }

        public Block(
            Header header, List<Expression> body, Expression endStatement,
            Token startToken = null, Token endToken = null)
        {
            Header = header;
            Statements = body;
            EndStatement = endStatement;

            StartToken = startToken ?? Header.StartToken;
            EndToken = endToken ?? EndStatement?.EndToken
                ?? (Statements.Count > 0 ? Statements[^1].EndToken : null);

            header.ParentBlockHint = this;
        }

        public override string ToString()
        {
            return $"({Statements.Count} statements) {Header}";
        }

        public override string GetTraceString()
        {
            return Header.GetTraceString();
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            yield return Header;

            foreach (Expression statement in Statements)
                yield return statement;

            if (EndStatement != null)
                yield return EndStatement;
        }

        public bool TryGetVariableInfo(string name, out VariableInfo result)
        {
            if (ScopeHint.VariableDeclarations.TryGetValue(name, out result))
                return true;

            if (ParentBlockHint != null && ParentBlockHint.TryGetVariableInfo(name, out result))
                return true;

            return false;
        }
    }
}
