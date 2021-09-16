using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Exceptions;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Language.Values;
using NSprak.Tokens;

using NSprak.Expressions.Types;

namespace NSprak.Expressions.Creation
{
    public static class Literals
    {
        public static LiteralGet Create(MatchIterator iterator)
        {
            iterator.AssertToken(out Token token);
            LiteralGet result = new LiteralGet(token);
            iterator.AssertEnd();

            return result;
        }

        public static LiteralArrayGet CreateArray(MatchIterator iterator)
        {
            Token start, end;

            iterator.AssertKeySymbol(Symbols.OpenSquareBracket, out _);
            start = (Token)iterator.Current;

            if (iterator.Next(out List<Expression> elements))
                iterator.MoveNext();

            else elements = new List<Expression>();

            iterator.AssertKeySymbol(Symbols.CloseSquareBracket, out _);
            end = (Token)iterator.Current;

            return new LiteralArrayGet(start, end, elements);
        }
    }
}
