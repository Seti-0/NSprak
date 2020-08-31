using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Exceptions;
using NSprak.Language;

namespace NSprak.Tokens
{
    public static class TokenAssertions
    {
        public static bool IsKeyWord(this Token token, string keyWord)
        {
            if (token.Type != TokenType.KeyWord)
                return false;

            return token.Content == keyWord;
        }

        public static bool IsKeySymbol(this Token token, char symbol)
        {
            if (token.Type != TokenType.KeySymbol)
                return false;

            if (token.Content.Length != 1)
                return false;

            return token.Content[0] == symbol;
        }

        public static void AssertType(this Token token, TokenType type)
        {
            if (token.Type != type)
                throw new TokenCheckException(token, $"{token} expected to be of type {type}");
        }

        public static string AssertComment(this Token token)
        {
            token.AssertType(TokenType.Comment);
            return token.Content;
        }

        public static string AssertKeyword(this Token token)
        {
            token.AssertType(TokenType.KeyWord);
            return token.Content;
        }

        public static void AssertKeyword(this Token token, string keyword)
        {
            if (keyword != token.AssertKeyword())
                throw new TokenCheckException(token, $"Expected keyword \"{keyword}\", found {token}");
        }

        public static char AssertKeySymbol(this Token token)
        {
            token.AssertType(TokenType.KeySymbol);

            if (token.Content.Length != 1)
                throw new TokenCheckException(token, $"KeySymbol {token} must have token.Content of length 1");

            return token.Content[0];
        }

        public static void AssertKeySymbol(this Token token, char keySymbol)
        {
            if (keySymbol != token.AssertKeySymbol())
                throw new TokenCheckException(token, $"Expected key symbol \"{keySymbol}\", found {token}");
        }

        public static SprakType AssertType(this Token token)
        {
            token.AssertType(TokenType.Type);

            if (SprakType.TryParse(token.Content, out SprakType result))
                return result;

            else throw new TokenCheckException(token, $"Failed to parse Sprak Type: {token.Type}");
        }

        public static string AssertName(this Token token)
        {
            token.AssertType(TokenType.Name);
            return token.Content;
        }

        public static Operator AssertOperator(this Token token)
        {
            token.AssertType(TokenType.Operator);

            if (Operator.TryParse(out Operator result, text: token.Content))
                return result;

            else throw new TokenCheckException(token, $"Failed to parse operator: {token}");
        }

        public static string AssertString(this Token token)
        {
            token.AssertType(TokenType.String);
            return token.Content;
        }

        public static double AssertNumber(this Token token)
        {
            token.AssertType(TokenType.Number);

            if (double.TryParse(token.Content, out double result))
                return result;

            else throw new TokenCheckException(token, $"Failed to parse number: {token}");
        }

        public static bool AssertBoolean(this Token token)
        {
            token.AssertType(TokenType.Boolean);

            if (bool.TryParse(token.Content, out bool result))
                return result;

            else throw new TokenCheckException(token, $"Failed to parse boolean: {token}");
        }
    }
}
