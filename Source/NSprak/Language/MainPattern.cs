using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Patterns;
using NSprak.Tokens;
using NSprak.Expressions.Creation;

namespace NSprak.Language
{
    using static PatternBuilder;

    public static class MainPattern
    {
        private static Pattern _pattern;

        public static Pattern Instance
        {
            get
            {
                if (_pattern == null)
                    _pattern = CreatePattern();

                return _pattern;
            }
        }

        private static Pattern CreatePattern()
        {
            /*
             *  PART ONE - EXPRESSIONS
             */

            Pattern expressionPattern = new Pattern("Expression");
            Pattern arrayPattern = new Pattern("Array");
            Pattern getPattern = new Pattern("Get");
            Pattern literalPattern = new Pattern("Literal");


            // Literals

            literalPattern.AddEntryPoints(
                Element(TokenType.Boolean).End(Literals.Create),
                Element(TokenType.Number).End(Literals.Create),
                Element(TokenType.String).End(Literals.Create)
                );

            /* Arrays. 
             * Form: [expr1, expr2, ..., exprN] 
             * Empty arrays are allowed with []
             */

            arrayPattern.AddEntryPoints(
                Array(
                    Element(Symbols.OpenSquareBracket),
                    Element(expressionPattern),
                    Element(Symbols.Comma),
                    Element(Symbols.CloseSquareBracket)
                    )
                .End(Literals.CreateArray)
                );

            /* Variable get, and function call (get) share a pattern
             * Variable form: name
             * Function call form: name(expr1, expr2, ..., exprN)
             */

            getPattern.AddEntryPoints(
                Element(TokenType.Name)
                .Break()
                .AllowEnd(Variables.CreateReference)
                .Array(
                    Element(Symbols.OpenBracket),
                    Element(expressionPattern),
                    Element(Symbols.Comma),
                    Element(Symbols.CloseBracket)
                    )
                .End(Functions.CreateCall)
                );

            /* Expression
             * The form here is complicated.
             * 
             * An expression can be elements X interspersed by operators,
             * and possibly surrounded by operators.
             * 
             * X in the above could:
             *  - A subexpression (in brackets)
             *  - A variable get or function call
             *  - A literal (including arrays)
             */

            // Leaving out unary operators for now, until a
            // when statement is introduced, I guess

            expressionPattern.SplitAndAddEntryPoints(
                Dummy()
                .Join(
                    Element(literalPattern),

                    Element(arrayPattern),
                    Element(getPattern),

                    Element(Symbols.OpenBracket)
                    .Then(expressionPattern)
                    .Then(Symbols.CloseBracket)
                    )
                .AllowEnd(ExpressionGroups.Create)
                .Then(TokenType.Operator)
                .AllowLoopback()
                );

            /*
             *  PART TWO - STATEMENTS
             */

            Pattern mainPattern = new Pattern("Main");

            mainPattern
                .AllowEmpty()
                .AddEntryPoints(

                // Some keywords

                Element(Keywords.Break).End(Commands.Create),
                Element(Keywords.Continue).End(Commands.Create),
                Element(Keywords.Else).End(Commands.Create),
                Element(Keywords.End).End(Commands.Create),

                Element(Keywords.Return).AllowEnd(Commands.CreateReturn)
                .Then(expressionPattern).End(Commands.CreateReturn),

                // If statement header

                Element(Keywords.If)
                .Then(expressionPattern)
                .End(ControlFlow.CreateIfHeader),

                // Loop statement header

                // I'm keeping only three of the four Sprak headers for the
                // sake of avoiding ambiguity, for now

                Element(Keywords.Loop)

                    // Infinite
                    .AllowEnd(ControlFlow.CreateLoopHeader)

                    .Then(TokenType.Name)
                    .Fork(
                        
                        // Range
                        Element(Keywords.From)
                        .Then(expressionPattern)
                        .Then(Keywords.To)
                        .Then(expressionPattern)
                        .End(ControlFlow.CreateLoopHeader),

                        // Foreach
                        Element(Keywords.In)
                        .Then(expressionPattern)
                        .End(ControlFlow.CreateLoopHeader)

                    ),

                // Names can begin two statements - function calls and variable assignments

                Element(TokenType.Name)
                .Fork(

                    // Variable assignment
                    // Form: myvar += expr
                    // (Where += can be any assignment op)
                    // Short form: myvar++
                    Element(TokenType.Operator)
                    .AllowEnd(Variables.CreateAssignment)
                    .Then(expressionPattern)
                    .End(Variables.CreateAssignment),

                    // Function call
                    // Form: myfunction(expr1, expr2, ..., exprN)
                    Array(
                        Element(Symbols.OpenBracket),
                        Element(expressionPattern),
                        Element(Symbols.Comma),
                        Element(Symbols.CloseBracket)
                        )
                    .End(Functions.CreateCall)

                    ),

                // Types have a similar two options - function headers and variable declarations
                Element(TokenType.Type)
                .Then(TokenType.Name)
                .Fork(

                    // Variable declaration:
                    // Form: type name = expr
                    // Or just: type name
                    Element(TokenType.Operator)
                    .Then(expressionPattern)
                    .End(Variables.CreateAssignment),

                    // Function header
                    // Form: type1 name1(type2 name2, ..., typeN nameN)
                    Array(
                        Element(Symbols.OpenBracket),
                        Element(TokenType.Type).Then(TokenType.Name),
                        Element(Symbols.Comma),
                        Element(Symbols.CloseBracket)
                        )
                    .End(Functions.CreateHeader)

                    )
                );

            return mainPattern;
        }
    }
}
