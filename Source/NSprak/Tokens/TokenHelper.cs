using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NSprak.Language;
using NSprak.Messaging;

namespace NSprak.Tokens
{

    public static class TokenHelper
    {
        private enum TokenPrototype
        {
            KeySymbol, Word, Number, String, Operator, Comment, Array
        }

        public static void TryParse(string line, out IList<RawToken> tokens)
        {
            // A big function with a goto statement - I had to try it at least once, for old time's sake

            int index = 0;
            List<RawToken> tokenList = new List<RawToken>();

            bool hasCurrent = index < line.Length;
            char currentChar = line[index];

            void Step()
            {
                index++;
                hasCurrent = index < line.Length;
                if (hasCurrent)
                    currentChar = line[index];
            }

            RawToken currentToken;

            // Walk the string one char at a time

            while (hasCurrent)
            {
                if (char.IsWhiteSpace(currentChar))
                {
                    Step();
                    continue;
                }

                // While there are still chars left, follow a three step process

                currentToken = new RawToken
                {
                    ColumnStart = index,
                    Content = ""
                };

                char stringBoundary = '"';

                // Step 1: identify the current char as being the start of a certain type of token
                // (Or break early if the char is unexpected)

                // Some token types look indentical to begin with - these share the same token "prototype"
                // For example, a keyword, type and variable name all share the prototype "Word"

                TokenPrototype? prototype = null;

                if (Symbols.IsKeyCharacter(currentChar)) prototype = TokenPrototype.KeySymbol;
                else if (Symbols.IsWordStart(currentChar)) prototype = TokenPrototype.Word;
                else if (Symbols.IsNumberStart(currentChar)) prototype = TokenPrototype.Number;
                else if (Symbols.IsStringStart(currentChar)) prototype = TokenPrototype.String;
                else if (Symbols.IsOperatorStart(currentChar)) prototype = TokenPrototype.Operator;
                else if (Symbols.IsCommentStart(currentChar)) prototype = TokenPrototype.Comment;

                if (prototype.HasValue)
                {
                    // Special case for strings: the first char will be a "string start"
                    // char (" or ') not part of the string
                    if (prototype == TokenPrototype.String)
                        // We do need which of " or ' it is for later
                        stringBoundary = currentChar;
                    else
                        currentToken.Content = currentChar.ToString();
                }
                else
                {
                    // It might be nice to make an unrecognized character token instead
                    // That would allow us to continue instead of break out of the line

                    currentToken.Error = true;
                    currentToken.ErrorMessage = Messages.UnrecognizedSymbols;
                    currentToken.Type = TokenType.Unknown;
                    currentToken.Content = "";

                    while (hasCurrent && Symbols.IsUnkownStart(currentChar))
                    {
                        currentToken.Content += currentChar;
                        Step();
                    }

                    currentToken.ColumnEnd = index;

                    tokenList.Add(currentToken);

                    continue;
                }

                // Step 2: keep stepping forward while the chars match the interior of
                // the current token prototype
                // (Append the current char to the token content at each step)

                Step();

                while (hasCurrent)
                {
                    switch (prototype)
                    {
                        case TokenPrototype.Word when !Symbols.IsWordCharacter(currentChar):
                        case TokenPrototype.Number when !Symbols.IsNumberCharacter(currentChar):
                        case TokenPrototype.Operator when !Symbols.IsOperatorCharacter(currentChar):
                        // Special case for key symbols: they have no interior
                        case TokenPrototype.KeySymbol:
                        // Special case for strings: two ends possible, it must match the start
                        case TokenPrototype.String when stringBoundary == currentChar:
                            goto CompleteToken;

                        default: break;
                    }

                    currentToken.Content += currentChar;
                    Step();
                }

                CompleteToken:;

                // Special case for strings: the last char will be the "string end"
                // char (" or '), not part of the string
                if (prototype == TokenPrototype.String)
                    Step();

                // Step 3: Identity the full type of the token from the completed content,
                // and add the completed token to the list.
                // (Break early if the token cannot be identified)

                currentToken.ColumnEnd = index;
                TokenType completedType;

                string content = currentToken.Content;

                switch (prototype)
                {
                    // Some words have immediately recognizable meaning

                    case TokenPrototype.Word when Keywords.IsKeyword(content): completedType = TokenType.KeyWord; break;
                    case TokenPrototype.Word when SprakType.IsType(content): completedType = TokenType.Type; break;
                    case TokenPrototype.Word when Value.IsBoolean(content): completedType = TokenType.Boolean; break;

                    // The rest can only be checked for validity at a higher level

                    case TokenPrototype.Word: completedType = TokenType.Name; break;

                    // Numbers and operators are recognized, but could still have an immediately recognizable error

                    case TokenPrototype.Operator:
                        completedType = TokenType.Operator;
                        CheckOperator(currentToken);
                        break;

                    case TokenPrototype.Number:
                        completedType = TokenType.Number;
                        CheckNumber(currentToken);
                        break;

                    // These will always be valid tokens

                    case TokenPrototype.KeySymbol: completedType = TokenType.KeySymbol; break;
                    case TokenPrototype.String: completedType = TokenType.String; break;
                    case TokenPrototype.Comment: completedType = TokenType.Comment; break;

                    // Ideally, this can't happen, unless I've made a mistake somewhere above

                    default:
                        string prototypeName = Enum.GetName(typeof(TokenPrototype), prototype);
                        string message = $"Token type not recognised: [{prototypeName}:{content}]";
                        throw new NotImplementedException(message);
                }


                currentToken.Type = completedType;
                tokenList.Add(currentToken);

            } // end main while 

            tokens = tokenList;
        }

        private static void CheckOperator(RawToken token)
        {
            if (Operator.IsOperator(text: token.Content))
                return;

            token.Error = true;
            token.ErrorMessage = Messages.UnrecognizedOperator;
            token.ErrorParams = new object[] { token.Content };
        }

        private static void CheckNumber(RawToken token)
        {
            if (Value.IsNumber(token.Content))
                return;

            token.Error = true;
            token.ErrorMessage = Messages.UnrecognizedNumber;
            token.ErrorParams = new object[] { token.Content };
        }
    }
}
