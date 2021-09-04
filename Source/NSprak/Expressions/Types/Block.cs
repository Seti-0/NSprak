using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Tokens;
using NSprak.Expressions.Structure;

namespace NSprak.Expressions.Types
{
    public abstract class Header : Expression
    {
        public abstract string FriendlyBlockName { get; }

        public bool RequiresScopeHint
        {
            get
            {
                bool? hint = ParentBlockHint?.VariableDeclarationsHint?.Count > 0;

                if (hint.HasValue)
                    return hint.Value;

                return true;
            }
        }
    }

    public class Block : Expression
    {
        private Token _startToken;
        private Token _endToken;

        public Header Header { get; }

        public IDictionary<string, VariableInfo> VariableDeclarationsHint { get; set; }

        public IReadOnlyList<Expression> Statements { get; }

        public override Token StartToken => _startToken ?? Header.StartToken;

        public override Token EndToken => _endToken ?? 
            (Statements.Count != 0 ? Statements[^1].EndToken : Header.EndToken);

        public Block(Header header, List<Expression> body, 
            Token startToken = null, Token endToken = null)
        {
            Header = header;
            header.ParentBlockHint = this;
            _startToken = startToken;
            _endToken = endToken;

            Statements = body;
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
        }

        public bool TryGetVariableInfo(string name, out VariableInfo result)
        {
            if (VariableDeclarationsHint.TryGetValue(name, out result))
                return true;

            if (ParentBlockHint != null && ParentBlockHint.TryGetVariableInfo(name, out result))
                return true;

            return false;
        }
    }
}
