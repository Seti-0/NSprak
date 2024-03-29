﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Expressions.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Functions.Signatures;

namespace NSprak.Expressions.Creation
{
    public static class Functions
    {
        public static FunctionCall Call(MatchIterator iterator)
        {
            // It would be cool in patterner v2 if the pattern was constructed in the same method as the finalizer.
            // For example, a "dummy" matchIterator could be passed through here and pick up the requirements, perhaps?
            // This would, of course, require the big feature of pattern simplification
            // It would also require defining a special "Expression" subpattern? At this point it would be better if everything were an expression, I guess

            iterator.AssertTokenType(TokenType.Name, out Token name);
            iterator.AssertKeySymbol(Symbols.OpenBracket, out _);
            Token open = (Token)iterator.Current;

            if (iterator.Next(out List<Expression> arguments))
                iterator.MoveNext();

            else arguments = new List<Expression>();

            iterator.AssertKeySymbol(Symbols.CloseBracket, out _);
            Token end = (Token)iterator.Current;

            iterator.AssertEnd();

            FunctionCall result = new FunctionCall(name, open, end, arguments);
            return result;
        }

        public static FunctionHeader Header(MatchIterator iterator)
        {
            iterator.AssertTokenType(TokenType.Type, out Token typeToken);
            iterator.AssertTokenType(TokenType.Name, out Token nameToken);
            iterator.AssertKeySymbol(Symbols.OpenBracket, out _);
            Token open = (Token)iterator.Current;

            CollectedParameters parameters;
            if (iterator.Next(out parameters))
                iterator.MoveNext();
            else
                parameters = new CollectedParameters();

            iterator.AssertKeySymbol(Symbols.CloseBracket, out _);
            Token end = (Token)iterator.Current;

            iterator.AssertEnd();

            FunctionTypeSignature typeSignature = new FunctionTypeSignature(parameters.Types);
            FunctionSignature signature = new FunctionSignature(
                FunctionSignature.Local, nameToken.Content, typeSignature);

            FunctionHeader result = new FunctionHeader(
                typeToken, nameToken, open, end, signature, parameters.Names);
            return result;
        }
    }
}
