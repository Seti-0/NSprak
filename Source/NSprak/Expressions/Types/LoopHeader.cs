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

        public Token NameToken { get; }

        public bool HasName => NameToken != null;

        public string Name => NameToken?.Content;

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

        public LoopHeader(Token loop)
        {
            loop.AssertKeyword(Keywords.Loop);
            LoopToken = loop;

            LoopType = LoopType.Infinite;
        }

        public LoopHeader(Token loop, Token nameToken,
            Expression start, Expression end)
        {
            nameToken.AssertName();
            loop.AssertKeyword(Keywords.Loop);
            
            LoopToken = loop;
            NameToken = nameToken;

            RangeStart = start;
            RangeEnd = end;

            LoopType = LoopType.Range;
        }

        public LoopHeader(Token loop, Expression array, Token nameToken = null)
        {
            loop.AssertKeyword(Keywords.Loop);
            nameToken.AssertName();

            LoopToken = loop;
            NameToken = nameToken;

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
    }
}
