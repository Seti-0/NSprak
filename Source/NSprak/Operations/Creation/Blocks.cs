using System;
using System.Collections.Generic;
using System.Text;
using NSprak.Expressions.Types;
using NSprak.Operations.Types;

namespace NSprak.Operations.Creation
{
    public static class Blocks
    {
        public static void GenerateCode(Block block, GeneratorContext builder)
        {
            builder.AddComment("Begin " + block.Header.FriendlyBlockName);

            builder.AddCode(block.Header);

            builder.AddComment("End " + block.Header.FriendlyBlockName);
        }
    }
}
