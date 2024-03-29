﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NSprak.Language
{
    public static class Symbols
    {
        public const char
            CommentStart = '#',
            StringBoundary = '\"',
            StringBoundaryAlternate = '\'',
            OpenBracket = '(',
            CloseBracket = ')',
            Comma = ',',
            DecimalPoint = '.',
            OpenSquareBracket = '[',
            CloseSquareBracket = ']',
            Minus = '-'
            ;

        public static readonly IReadOnlyList<char> KeySymbols;

        private readonly static HashSet<char> operatorCharacters;

        static Symbols()
        {
            KeySymbols = new char[]
            {
                OpenBracket, CloseBracket, OpenSquareBracket, CloseSquareBracket, Comma, Comma
            };

            operatorCharacters = new HashSet<char>(Operator.All.SelectMany(x => x.Text.ToCharArray()));
        }

        public static bool IsKeyCharacter(char symbol)
        {
            return KeySymbols.Contains(symbol);
        }

        public static bool IsWordStart(char symbol)
        {
            return char.IsLetter(symbol) 
                || symbol == '@'
                || symbol == '_';
        }

        public static bool IsWordCharacter(char symbol)
        {
            return char.IsLetterOrDigit(symbol) 
                || symbol == '@'
                || symbol == '_';
        }

        public static bool IsValidWord(string text)
        {
            return IsWordStart(text[0])
                && text[1..].All(c => IsWordCharacter(c));
        }

        public static bool IsStringStart(char symbol)
        {
            return symbol == StringBoundary || symbol == StringBoundaryAlternate;
        }

        public static bool IsNumberStart(char symbol)
        {
            // Question: does Sprak support decimal points at the start?
            return char.IsDigit(symbol) ;
        }

        public static bool IsNumberCharacter(char symbol)
        {
            // For now, simple decimals only. It might be worth implementing scientific notation
            // and alternate bases at some point, if sprak supports them
            return char.IsDigit(symbol) || symbol == DecimalPoint;

            // Also for goodness sake, it would be worth fixing the fact that 2 decimal points
            // panicks the compiler...
        }

        public static bool IsValidNumber(string text)
        {
            return IsNumberStart(text[0])
                && text[1..].All(c => IsNumberCharacter(c))
                && double.TryParse(text, out _);
        }

        public static bool IsOperatorStart(char symbol)
        {
            return IsOperatorCharacter(symbol);
        }

        public static bool IsOperatorCharacter(char symbol)
        {
            return operatorCharacters.Contains(symbol);
        }

        public static bool IsCommentStart(char symbol)
        {
            return symbol == CommentStart;
        }

        public static bool IsUnkownStart(char symbol)
        {
            // Yup, I really need to remake this for general token types
            // as opposed to hardcoding each l'il thing

            if (IsKeyCharacter(symbol))
                return false;

            if (IsWordStart(symbol))
                return false;

            if (IsNumberStart(symbol))
                return false;

            if (IsStringStart(symbol))
                return false;

            if (IsOperatorStart(symbol))
                return false;

            if (IsCommentStart(symbol))
                return false;

            return true;
        }
    }
}
