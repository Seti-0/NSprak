﻿using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Expressions.Patterns;
using NSprak.Expressions.Creation;

namespace NSprak.Language
{
    using static NSprak.Expressions.Patterns.RuntimeTrace;
    using static PatternSyntax;

    public class MainPattern
    {
        private static Pattern _instance;

        public static Pattern Instance
        {
            get 
            {
                if (_instance == null)
                    _instance = Create();

                _instance.Validate();
                new PatternTextHelper()
                    .Visit(_instance);

                return _instance;
            }
        }

        private static Pattern Create()
        {
            // & has higher precedence than |
            // (Similar to how * has higher precendece than +)

            // That means any option groups need brackets around them.

            /*
                PART ONE
                Expressions
            */

            Pattern Expression = Pattern("Expression");

            Pattern ExpressionTuple = Pattern("Expression Tuple");

            Pattern Literal = Pattern("Literal");
            Pattern ArrayLiteral = Pattern("Array Literal");
            Pattern Get = Pattern("Get");
            Pattern Index = Pattern("Index");

            Literal.Value = (Boolean | Number | Text) 
                & EndWith(Literals.Create);

            ArrayLiteral.Value = KeySymbol.OpenSquareBracket
                & ExpressionTuple
                & KeySymbol.CloseSquareBracket
                & EndWith(Literals.CreateArray);

            Get.Value = Name
                & Allow(EndWith(Variables.CreateReference))
                & KeySymbol.OpenBracket
                & ExpressionTuple
                & KeySymbol.CloseBracket
                & EndWith(Functions.CreateCall);

            Index.Value = KeySymbol.OpenSquareBracket
                & Expression
                & KeySymbol.CloseSquareBracket
                & Allow(EndWith(null))
                & Loopback;

            ExpressionTuple.Value = Expression
                & Allow(EndWith(Collection.Arguments))
                & KeySymbol.Comma
                & Loopback;

            Expression.Value = (
                    OperatorToken & Expression
                    | Literal
                    | ArrayLiteral
                    | Get
                    | KeySymbol.OpenBracket
                        & Expression
                        & KeySymbol.CloseBracket
                    )
                & (
                    Index & OperatorToken
                    | OperatorToken
                    | EndWith(ExpressionGroups.Create)
                )
                & Allow(EndWith(ExpressionGroups.Create))
                & Loopback;

            /*
                PART TWO
                Statements
            */

            Pattern Statement = Pattern("Statement");
            Pattern DeclarationTuple = Pattern("Declaration Tuple");

            DeclarationTuple.Value = 
                Type & Name
                & Allow(EndWith(Collection.Parameters))
                & KeySymbol.Comma
                & Loopback;

            Statement.Value =
                
                // Allow an empty file

                Empty & EndWith(_ => null)
                
                // Standalone keywords

                | (Keyword.Continue | Keyword.End | Keyword.Break)
                    & EndWith(Commands.Create)

                | Keyword.Return 
                    & Allow(Expression)
                    & EndWith(Commands.CreateReturn)

                // Assignments and calls

                | Name
                    & (
                        OperatorToken
                            & Allow(EndWith(Variables.CreateAssignment))
                            & Expression
                            & EndWith(Variables.CreateAssignment)

                        | KeySymbol.OpenBracket
                            & ExpressionTuple
                            & KeySymbol.CloseBracket
                            & EndWith(Functions.CreateCall)
                    )

                // If, Else and Else If statements.

                | Keyword.If & Expression & EndWith(ControlFlow.CreateIfHeader)
                | Keyword.Else 
                    & Allow(EndWith(null))
                    & Keyword.If & Expression & EndWith(null)

                // Loop statements

                | Keyword.Loop 
                    & Allow(EndWith(ControlFlow.CreateLoopHeader))
                    & Expression
                    & Allow(EndWith(ControlFlow.CreateLoopHeader))
                    & (
                        Keyword.From 
                            & Expression 
                            & Keyword.To 
                            & Expression
                        | Keyword.In 
                            & Expression
                    )
                    & EndWith(ControlFlow.CreateLoopHeader)

                // Declarations of variables and functions

                | Type & Name 
                    & (
                        OperatorToken & Expression & EndWith(Variables.CreateAssignment)
                        | KeySymbol.OpenBracket
                            & DeclarationTuple
                            & KeySymbol.CloseBracket
                            & EndWith(Functions.CreateHeader)
                    );

            return Statement;
        }
    }
}
