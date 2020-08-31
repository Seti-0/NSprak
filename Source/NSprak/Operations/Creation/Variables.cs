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
            builder.AddOp(new VariableGet(reference.Name));
        }

        public static void GenerateCode(VariableAssignment assignment, GeneratorContext builder)
        {
            if (assignment.IsDeclaration)
            {
                if (assignment.HasValue)
                    builder.AddCode(assignment.Value);

                else builder.AddOp(new LiteralValue(assignment.DeclarationType.Default()));

                builder.AddOp(new VariableCreate(assignment.Name));
            }
            else
            {
                if (assignment.BuiltInFunctionHint != null)
                {
                    switch (assignment.Operator.Inputs)
                    {
                        case OperatorSide.Both:
                            if (assignment.Value != null) builder.AddCode(assignment.Value);
                            builder.AddOp(new VariableGet(assignment.Name));
                            break;

                        case OperatorSide.Left:
                            builder.AddOp(new VariableGet(assignment.Name));
                            break;

                        case OperatorSide.Right:
                            if (assignment.Value != null) builder.AddCode(assignment.Value);
                            break;
                    }

                    builder.AddOp(new CallBuiltIn(assignment.BuiltInFunctionHint));

                    builder.AddOp(new VariableSet(assignment.Name));
                }
            }
        }
    }
}
