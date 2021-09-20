using System.Collections.Generic;
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
                & Allow(ExpressionTuple)
                & KeySymbol.CloseSquareBracket
                & EndWith(Literals.CreateArray);

            Get.Value = Name
                & Allow(EndWith(Variables.CreateReference))
                & KeySymbol.OpenBracket
                & Allow(ExpressionTuple)
                & KeySymbol.CloseBracket
                & EndWith(Expressions.Creation.Functions.Call);

            Index.Value = KeySymbol.OpenSquareBracket
                & Expression
                & KeySymbol.CloseSquareBracket
                & Allow(EndWith(Collection.Indices))
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
                & Allow(Index)
                & Allow(EndWith(ExpressionGroups.Create))
                & OperatorToken
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
                    & EndWith(Commands.Return)

                // Assignments and calls

                | Name
                    & (
                        OperatorToken
                            & Allow(EndWith(Variables.Assignment))
                            & Expression
                            & EndWith(Variables.Assignment)

                        | KeySymbol.OpenBracket
                            & Allow(ExpressionTuple)
                            & KeySymbol.CloseBracket
                            & EndWith(Expressions.Creation.Functions.Call)
                    )

                // If, Else and Else If statements.

                | Keyword.If & Expression & EndWith(ControlFlow.If)
                | Keyword.Else 
                    & Allow(EndWith(ControlFlow.Else))
                    & Keyword.If & Expression & EndWith(ControlFlow.ElseIf)

                // Loop statements

                | Keyword.Loop 
                    & Allow(EndWith(ControlFlow.Loop))
                    & Expression
                    & Allow(EndWith(ControlFlow.Loop))
                    & (
                        Keyword.From 
                            & Expression 
                            & Keyword.To 
                            & Expression
                        | Keyword.In 
                            & Expression
                    )
                    & EndWith(ControlFlow.Loop)

                // Declarations of variables and functions

                | Type & Name
                    & (
                        OperatorToken & Expression & EndWith(Variables.Assignment)
                        | KeySymbol.OpenBracket
                            & Allow(DeclarationTuple)
                            & KeySymbol.CloseBracket
                            & EndWith(Expressions.Creation.Functions.Header)
                    );

            return Statement;
        }
    }
}
