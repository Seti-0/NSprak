using NSprak.Language;
using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NSprak.Expressions.Types
{
    public class Return : Expression
    {
        public Token ReturnToken { get; }

        public bool HasValue => Value != null;

        public Expression Value { get; }

        public override Token StartToken => ReturnToken;

        public override Token EndToken => Value?.EndToken ?? ReturnToken;

        public Return(Token returnToken, Expression expression)
        {
            returnToken.AssertKeyword(Keywords.Return);
            ReturnToken = returnToken;

            Value = expression;
        }

        public override string ToString()
        {
            string result = "return";

            if (Value != null)
                result += " " + Value.ToString();

            return result;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            if (Value != null)
                yield return Value;
        }
    }
}
