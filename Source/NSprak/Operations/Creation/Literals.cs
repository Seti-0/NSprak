using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Operations.Types;
using NSprak.Expressions;
using NSprak.Expressions.Types;

namespace NSprak.Operations.Creation
{
    public static class Literals
    {
        public static void GenerateCode(LiteralArrayGet literal, GeneratorContext builder)
        {
            foreach (Expression element in literal.Elements)
                builder.AddCode(element);

            builder.AddOp(new ArrayValue(literal.Elements.Count));
        }

        public static void GenerateCode(LiteralGet literal, GeneratorContext builder)
        {
            builder.AddOp(new LiteralValue(literal.Value));
        }
    }
}
