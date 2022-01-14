using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public enum LoopType
    {
        Infinite, Foreach, Range
    }

    public class LoopHeader : Header
    {
        public LoopType LoopType { get; private set; }

        public bool IsForeach => LoopType == LoopType.Foreach;

        public bool IsInfinite => LoopType == LoopType.Infinite;

        public bool IsRange => LoopType == LoopType.Range;

        public Token LoopToken { get; }

        public Expression NameExpression { get; }

        public Token NameToken => NameExpression.StartToken;

        public Token FromToken { get; }

        public Token ToToken { get; }

        public Token InToken { get; }

        public bool HasName => NameExpression != null;

        public string Name => NameExpression?.StartToken.Content;

        public override Token StartToken => LoopToken;

        public override Token EndToken
        {
            get => LoopType switch
            {
                LoopType.Infinite => LoopToken,
                LoopType.Range => RangeEnd.EndToken,
                LoopType.Foreach => Array.EndToken,
                _ => throw new NotSupportedException("Unrecognized loop type")
            };
        }

        public Expression Array { get; }

        public Expression RangeStart { get; }

        public Expression RangeEnd { get; }

        public override string FriendlyBlockName => "loop";

        public string IndexNameHint { get; set; }

        public LoopHeader(Token loop)
        {
            loop.AssertKeyword(Keywords.Loop);
            LoopToken = loop;

            LoopType = LoopType.Infinite;
            loop.ExpressionHint = this;
        }

        public LoopHeader(Token loop, Expression name, Token from, 
            Expression start, Token to, Expression end)
        {
            loop.AssertKeyword(Keywords.Loop);
            
            LoopToken = loop;
            NameExpression = name;
            FromToken = from;
            RangeStart = start;
            ToToken = to;
            RangeEnd = end;

            LoopType = LoopType.Range;
            loop.ExpressionHint = this;
            from.ExpressionHint = this;
            to.ExpressionHint = this;
        }

        public LoopHeader(Token loop, Expression array)
        {
            loop.AssertKeyword(Keywords.Loop);

            LoopToken = loop;
            Array = array;

            LoopType = LoopType.Foreach;
        }

        public LoopHeader(Token loop, Expression name, 
            Token inToken, Expression array)
        {
            loop.AssertKeyword(Keywords.Loop);

            LoopToken = loop;
            NameExpression = name;
            InToken = inToken;
            Array = array;

            LoopType = LoopType.Foreach;
        }

        public override string ToString()
        {
            return LoopType switch
            {
                LoopType.Infinite => "loop",
                LoopType.Range => $"loop from {RangeStart} to {RangeEnd}",

                LoopType.Foreach =>
                    HasName ? $"loop {Name} in {Array}" : $"loop {Array}",

                _ => throw new NotSupportedException("Unrecognized loop type")
            };
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            if (Array != null)
                yield return Array;

            if (RangeStart != null)
                yield return RangeStart;

            if (RangeEnd != null)
                yield return RangeEnd;
        }

        public override IEnumerable<Token> GetTokens()
        {
            yield return LoopToken;

            if (NameToken != null)
                yield return NameToken;

            if (FromToken != null)
                yield return FromToken;

            if (ToToken != null)
                yield return ToToken;

            if (InToken != null)
                yield return InToken;
        }
    }
}
