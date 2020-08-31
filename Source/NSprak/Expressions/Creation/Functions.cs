using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Expressions.Types;
using NSprak.Expressions.Patterns;
using NSprak.Language;
using NSprak.Tokens;
using NSprak.Language.Builtins;

namespace NSprak.Expressions.Creation
{
    public static class Functions
    {
        public static FunctionCall CreateCall(MatchIterator iterator)
        {
            // It would be cool in patterner v2 if the pattern was constructed in the same method as the finalizer.
            // For example, a "dummy" matchIterator could be passed through here and pick up the requirements, perhaps?
            // This would, of course, require the big feature of pattern simplification
            // It would also require defining a special "Expression" subpattern? At this point it would be better if everything were an expression, I guess

            iterator.AssertTokenType(TokenType.Name, out Token name);
            iterator.AssertKeySymbol(Symbols.OpenBracket);

            List<Expression> arguments = new List<Expression>();

            if (iterator.NextIsExpression(out Expression argument))
            {
                arguments.Add(argument);

                while (iterator.NextIsKeySymbol(Symbols.Comma))
                {
                    iterator.AssertExpression(out argument);
                    arguments.Add(argument);
                }
            }

            iterator.AssertKeySymbol(Symbols.CloseBracket);
            Token end = (Token)iterator.Current;

            iterator.AssertEnd();

            FunctionCall result = new FunctionCall(name, end, arguments);
            return result;
        }

        public static FunctionHeader CreateHeader(MatchIterator iterator)
        {
            iterator.AssertTokenType(TokenType.Type, out Token typeToken);
            iterator.AssertTokenType(TokenType.Name, out Token nameToken);
            iterator.AssertKeySymbol(Symbols.OpenBracket);

            List<string> parameterNames = new List<string>();
            List<SprakType> parameterTypes = new List<SprakType>();

            if (iterator.NextIsType(out SprakType paramType))
            {
                iterator.AssertTokenType(TokenType.Name, out Token paramName);
                parameterNames.Add(paramName.Content);
                parameterTypes.Add(paramType);

                while (iterator.NextIsKeySymbol(Symbols.Comma))
                {
                    iterator.AssertType(out paramType);
                    iterator.AssertTokenType(TokenType.Name, out paramName);
                    parameterNames.Add(paramName.Content);
                    parameterTypes.Add(paramType);
                }
            }

            iterator.AssertKeySymbol(Symbols.CloseBracket);
            Token end = (Token)iterator.Current;

            iterator.AssertEnd();

            FunctionTypeSignature typeSignature = new FunctionTypeSignature(parameterTypes);
            FunctionSignature signature = new FunctionSignature(null, nameToken.Content, typeSignature);

            FunctionHeader result = new FunctionHeader(
                typeToken, nameToken, end, signature, parameterNames);
            return result;
        }
    }
}
