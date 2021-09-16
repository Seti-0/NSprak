using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions.Creation
{
    public static class Collection
    {
        public static List<CollectedIndex> Indices(MatchIterator iterator)
        {
            List<CollectedIndex> indices = new List<CollectedIndex>();

            iterator.AssertNext();

            while (iterator.HasNext)
            {
                iterator.AssertKeySymbol(Symbols.OpenSquareBracket, out Token open);
                iterator.AssertExpression(out Expression index);
                iterator.AssertKeySymbol(Symbols.CloseSquareBracket, out Token close);

                CollectedIndex entry = new CollectedIndex
                {
                    Open = open,
                    Index = index,
                    Close = close
                };

                indices.Add(entry);
            }

            return indices;
        } 

        public static List<Expression> Arguments(MatchIterator iterator)
        {
            List<Expression> arguments = new List<Expression>();

            if (iterator.AtEnd())
                return arguments;

            do
            {
                iterator.AssertExpression(out Expression argument);
                arguments.Add(argument);
            }
            while (iterator.NextIsKeySymbol(Symbols.Comma));

            iterator.AssertEnd();

            return arguments;
        }

        public static CollectedParameters Parameters(MatchIterator iterator)
        {
            List<SprakType> parameterTypes = new List<SprakType>();
            List<string> parameterNames = new List<string>();
            CollectedParameters result 
                = new CollectedParameters(parameterTypes, parameterNames);

            if (iterator.AtEnd())
                return result;

            do
            {
                iterator.AssertType(out SprakType type);
                iterator.AssertName(out string name);
                parameterTypes.Add(type);
                parameterNames.Add(name);
            }
            while (iterator.NextIsKeySymbol(Symbols.Comma));

            iterator.AssertEnd();

            return result;
        }
    }
}
