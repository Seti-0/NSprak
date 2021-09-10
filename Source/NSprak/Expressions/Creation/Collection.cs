using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Patterns;
using NSprak.Language;

namespace NSprak.Expressions.Creation
{
    public class CollectedParameters
    {
        public List<SprakType> Types;
        public List<string> Names;

        public CollectedParameters()
        {
            Types = new List<SprakType>();
            Names = new List<string>();
        }

        public CollectedParameters(List<SprakType> types, List<string> names)
        {
            Types = types;
            Names = names;
        }

        public override string ToString()
        {
            List<string> items = new List<string>();

            for (int i = 0; i < Types.Count; i++)
            {
                string item = Types[i].Text;
                item += " " + Names[i];
                items.Add(item);
            }

            string result = string.Join(", ", items);
            return result;
        }
    }

    public static class Collection
    {
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
