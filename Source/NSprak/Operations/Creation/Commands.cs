using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Operations.Types;
using NSprak.Language;
using NSprak.Expressions.Types;

namespace NSprak.Operations.Creation
{
    public static class Commands
    {
        public static void GenerateCode(Expressions.Types.Return statement, GeneratorContext builder)
        {
            if (statement.HasValue)
                builder.AddCode(statement.Value);

            builder.AddOp(new Types.Return());
        }

        public static void GenerateCode(Command command, GeneratorContext builder)
        {
            string label = null;

            switch (command.Keyword)
            {
                case Keywords.Break:

                    if (builder.BreakLabels.Count == 0)
                        builder.ThrowError("Encountered break statement outside of loop");

                    label = builder.BreakLabels.Peek();
                    break;

                case Keywords.Continue:

                    if (builder.ContinueLabels.Count == 0)
                        builder.ThrowError("Encountered continue statement outside of loop");

                    label = builder.ContinueLabels.Peek();
                    break;

                default:

                    builder.ThrowError("Encountered unexpected command expression");
                    break;
            }

            if (label != null)
                builder.AddOp(new JumpLabel(label));
        }
    }
}
