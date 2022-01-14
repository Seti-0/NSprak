using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using NSprak.Functions.Signatures;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public class FunctionHeader : Header
    {
        public FunctionSignature Signature { get; }

        public IReadOnlyList<string> ParameterNames { get; }

        public SprakType ReturnType { get; }

        public int ParameterCount => ParameterNames.Count;

        public string Name => Signature.Name;

        public override string FriendlyBlockName => $"function: {Signature}";

        public Token TypeToken { get; }

        public Token NameToken { get; }

        public Token OpeningBracket { get; }

        public Token ClosingBracket { get; }

        public override Token StartToken => TypeToken;

        public override Token EndToken => ClosingBracket;

        public IReadOnlyList<SprakType> ParameterTypes => Signature.TypeSignature.Parameters;

        public FunctionHeader(Token typeToken, Token nameToken, Token openToken, Token endToken, 
            FunctionSignature signature, IReadOnlyList<string> parameterNames)
        {
            nameToken.AssertName();
            endToken.AssertKeySymbol(Symbols.CloseBracket);
            ReturnType = typeToken.AssertType();

            TypeToken = typeToken;
            NameToken = nameToken;
            ClosingBracket = endToken;

            Signature = signature;
            ParameterNames = parameterNames;

            NameToken.Hints |= TokenHints.Function;

            typeToken.ExpressionHint = this;
            nameToken.ExpressionHint = this;
            endToken.ExpressionHint = this;
        }

        public override string ToString()
        {
            return $"{ReturnType.Text} {Signature}";
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Enumerable.Empty<Expression>();
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return TypeToken;
            yield return NameToken;
            yield return OpeningBracket;
            yield return ClosingBracket;
        }
    }
}
