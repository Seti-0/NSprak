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

            builder.AddOp(new Types.Return(), statement.ReturnToken);
        }

        public static void GenerateCode(Command command, GeneratorContext builder)
        {
            string label = null;

            switch (command.Keyword)
            {
                case Keywords.End:
                    // End keywords were used to make the tree. They exist as 
                    // statements beyond that only so that the tree (and editor
                    // functions that depend on the tree) are aware of them.
                    return;

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

            if (label == null)
                throw new Exception("Missing label");


            Header ancestor = command.ParentBlockHint.Header;
            while (true)
            {
                // There might be intermediate if blocks that have scopes that
                // need ending. Also, the loop iteration we are breaking/skipping
                // might have one too.

                if (ancestor.RequiresScopeHint)
                    builder.AddOp(new ScopeEnd(), command.Token);

                if (ancestor is LoopHeader loopHeader)
                { 
                    if (command.Keyword == Keywords.Continue
                        && loopHeader.IsRange)
                    {
                        // For from-to loops, the increment is done at the end 
                        // of the loop, and needs to be done here too. (loop-in
                        // loops do the incrementing at the beginning)
                        builder.AddOp(new Increment(loopHeader.IndexNameHint), 
                            command.Token);
                    }

                    break;
                }

                // The parent of a header is the block it is a header of 
                // (confusing, when one puts it that way)

                // So to get the parent block of the current block we need
                // the parent of the parent

                ancestor = command.ParentBlockHint.ParentBlockHint?.Header;
            }

            if (ancestor == null)
                throw new Exception("Unable to locate continue command loop header");

            builder.AddOp(new JumpLabel(label), command.Token);
        }
    }
}
