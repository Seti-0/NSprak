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
                if (assignment.Value != null) builder.AddCode(assignment.Value);

                if (assignment.BuiltInFunctionHint != null)
                {
                    builder.AddOp(new VariableGet(assignment.Name), assignment.NameToken);
                    builder.AddOp(new CallBuiltIn(assignment.BuiltInFunctionHint), assignment.OperatorToken);
                }

                if (assignment.Indices.Count == 0)
                    builder.AddOp(new VariableSet(assignment.Name), assignment.NameToken);

                else if (assignment.Indices.Count > 1)
                    throw new NotImplementedException("Only one level of indexing is supported at the moment");

                else
                {
                    builder.AddCode(assignment.Indices[0].Index);
                    builder.AddOp(new ArrayElementSet(assignment.Name), 
                        assignment.NameToken);
                }
            }
        }
    }
}
