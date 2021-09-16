using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Operations.Types;
using NSprak.Language;
using NSprak.Operations;
using NSprak.Expressions.Types;

namespace NSprak.Operations.Creation
{
    public static class Variables
    {
        public static void GenerateCode(VariableReference reference, GeneratorContext builder)
        {
            builder.AddOp(new VariableGet(reference.Name), reference.Token);
        }

        public static void GenerateCode(Indexer indexer, GeneratorContext builder)
        {
            builder.AddCode(indexer.SourceExpression);
            builder.AddCode(indexer.IndexExpression);
            builder.AddOp(new ArrayElementGet(), indexer.OpeningBracket);
        }

        public static void GenerateCode(VariableAssignment assignment, GeneratorContext builder)
        {
            if (assignment.IsDeclaration)
            {
                if (assignment.HasValue)
                    builder.AddCode(assignment.Value);

                else builder.AddOp(new LiteralValue(assignment.DeclarationType.Default()), assignment.TypeToken);

                builder.AddOp(new VariableCreate(assignment.Name), assignment.NameToken);
            }
            else
            {
                if (assignment.BuiltInFunctionHint != null)
                {
                    switch (assignment.Operator.Inputs)
                    {
                        case OperatorSide.Both:
                            if (assignment.Value != null) builder.AddCode(assignment.Value);
                            builder.AddOp(new VariableGet(assignment.Name), assignment.NameToken);
                            break;

                        case OperatorSide.Left:
                            builder.AddOp(new VariableGet(assignment.Name), assignment.NameToken);
                            break;

                        case OperatorSide.Right:
                            if (assignment.Value != null) builder.AddCode(assignment.Value);
                            break;
                    }

                    builder.AddOp(new CallBuiltIn(assignment.BuiltInFunctionHint), assignment.OperatorToken);

                    builder.AddOp(new VariableSet(assignment.Name), assignment.NameToken);
                }
            }
        }
    }
}
