using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public class Command : SingleTokenExpression
    {
        public string Keyword => Token.Content;

        public Command(Token keywordToken) : base(keywordToken)
        {
            keywordToken.AssertKeyword();
        }

        public override string ToString()
        {
            return Keyword;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Enumerable.Empty<Expression>();
        }
    }
}
