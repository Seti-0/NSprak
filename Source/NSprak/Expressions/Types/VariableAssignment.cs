﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Language;
using NSprak.Language.Builtins;
using NSprak.Operations;
using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public class VariableAssignment : Expression
    {
        public bool IsDeclaration => DeclarationType != null;

        public SprakType DeclarationType { get; }

        public Token NameToken { get; }

        public Token OperatorToken { get; }

        public Token TypeToken { get; }

        public string Name => NameToken.Content;

        public Operator Operator { get; }

        public Expression Value { get; }

        public BuiltInFunction BuiltInFunctionHint { get; set; }

        public Func<Op> OpHint { get; set; }

        public bool HasValue => Value != null;

        public override Token StartToken => TypeToken ?? NameToken;

        public override Token EndToken => Value?.EndToken ?? OperatorToken;

        public VariableAssignment(Token typeToken, Token nameToken, Token operatorToken, Expression value)
        {
            nameToken.AssertName();
            Operator = operatorToken.AssertOperator();
            DeclarationType = typeToken?.AssertType();

            TypeToken = typeToken;
            NameToken = nameToken;
            OperatorToken = operatorToken;

            Value = value;
        }

        public override string ToString()
        {
            string result = Name;

            if (Value == null)
                result += Operator.Text;
            else
                result = $"{result} {Operator.Text} {Value}";

            if (IsDeclaration)
                result = $"{DeclarationType.Text} {result}";

            return result;
        }

        public override IEnumerable<Expression> GetSubExpressions()
        {
            if (Value != null)
                yield return Value;
        }
    }
}
