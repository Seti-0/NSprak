using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSprak.Operations.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Operations;
using NSprak.Functions.Signatures;
using NSprak.Functions;

namespace NSprak.Expressions.Types
{
    public class FunctionCall : Expression
    {
        public FunctionSignature UserFunctionHint { get; set; }

        public BuiltInFunction BuiltInFunctionHint { get; set; }

        public string Name => NameToken.Content;

        public List<Expression> Arguments { get; }

        public Token NameToken { get; }

        public Token OpeningBracketToken { get; }

        public Token ClosingBracketToken { get; }

        public override Token StartToken => NameToken;

        public override Token EndToken => ClosingBracketToken;

        public FunctionCall(Token nameToken, Token openingToken, Token endToken, List<Expression> arguments)
        {
            nameToken.AssertName();
            endToken.AssertKeySymbol(Symbols.CloseBracket);

            NameToken = nameToken;
            OpeningBracketToken = openingToken;
            ClosingBracketToken = endToken;

            Arguments = arguments;

            NameToken.Hints |= TokenHints.Function;

            nameToken.ExpressionHint = this;
            openingToken.ExpressionHint = this;
            endToken.ExpressionHint = this;
        }

        public override string ToString()
        {
            string arguments = string.Join(",", Arguments.Select(x => x.ToString()));
            return $"{Name}({arguments})";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Arguments;
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return NameToken;
            yield return OpeningBracketToken;
            yield return ClosingBracketToken;
        }
    }
}
