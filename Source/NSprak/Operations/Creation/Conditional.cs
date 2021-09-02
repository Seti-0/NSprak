using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Operations.Types;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Language.Values;

namespace NSprak.Operations.Creation
{
    public static class Conditional
    {
        public static void GenerateCode(IfHeader header, GeneratorContext builder)
        {
            if (header.RequiresScopeHint)
                builder.AddOp(new ScopeBegin());

            string endLabel = builder.DeclareLabel("endIf");

            builder.AddCode(header.Condition);

            builder.AddOp(new JumpLabelConditionalNegated(endLabel));

            foreach (Expression statement in header.ParentHint.Statements)
                builder.AddCode(statement);

            builder.SetLabelToNext(endLabel);

            if (header.RequiresScopeHint)
                builder.AddOp(new ScopeEnd());
        }

        public static void GenerateCode(LoopHeader header, GeneratorContext builder)
        {
            bool requiresScopes = header.RequiresScopeHint;

            if (requiresScopes)
                builder.AddOp(new ScopeBegin());

            string endLabel = builder.DeclareLabel("LoopEnd");
            builder.BreakLabels.Push(endLabel);

            string continueLabel = builder.DeclareLabel("LoopContinue");
            builder.ContinueLabels.Push(continueLabel);

            string indexName = null;

            if (header.IsInfinite)
                builder.SetLabelToNext(continueLabel);

            else if (header.IsRange)
            {
                builder.PushIndex();
                indexName = header.Name;

                // For the end expression, don't use a variable if a literal could be used instead
                // Actually, it would be better if this were generalized to statements that translate to a single
                // op, but that is an optimization for another day.
                Op endOp;

                if (header.RangeEnd is LiteralGet end)
                    endOp = new LiteralValue(end.Value);

                else
                {
                    string endName = builder.GetIndexedName("end");
                    builder.AddCode(header.RangeEnd);
                    builder.AddOp(new VariableCreate(endName));

                    endOp = new VariableGet(endName);
                }

                builder.AddCode(header.RangeStart);
                builder.AddOp(new VariableCreate(indexName));

                builder.SetLabelToNext(continueLabel);

                builder.AddOp(new VariableGet(indexName));
                builder.AddOp(endOp);
                builder.AddOp(new GreaterThan());
                builder.AddOp(new JumpLabelConditional(endLabel));
            }
            else
            {
                builder.PushIndex();
                indexName = builder.GetIndexedName("index");
                string arrayName = builder.GetIndexedName("array");
                string countName = builder.GetIndexedName("count");
                string currentName = header.Name ?? "@";

                builder.AddCode(header.Array);
                builder.AddOp(new VariableCreate(arrayName));

                builder.AddOp(new ArrayCount(arrayName));
                builder.AddOp(new VariableCreate(countName));

                builder.AddOp(new LiteralValue(new SprakNumber(0)));
                builder.AddOp(new VariableCreate(indexName));

                builder.AddOp(new LiteralValue(SprakUnit.Value));
                builder.AddOp(new VariableCreate(currentName));

                builder.SetLabelToNext(continueLabel);

                builder.AddOp(new VariableGet(indexName));
                builder.AddOp(new VariableGet(countName));
                builder.AddOp(new GreaterThanOrEqualTo());
                builder.AddOp(new JumpLabelConditional(endLabel));

                builder.AddOp(new VariableGet(arrayName));
                builder.AddOp(new VariableGet(indexName));
                builder.AddOp(new ArrayElementGet());
                builder.AddOp(new VariableSet(currentName));

                builder.AddOp(new Increment(indexName));
            };

            if (requiresScopes)
                builder.AddOp(new ScopeBegin());

            foreach (Expression statement in header.ParentHint.Statements)
                builder.AddCode(statement);

            if (requiresScopes)
                builder.AddOp(new ScopeEnd());

            if (header.IsRange)
                builder.AddOp(new Increment(indexName));

            builder.AddOp(new JumpLabel(continueLabel));
            builder.SetLabelToNext(endLabel);

            if (!header.IsInfinite)
                builder.PopIndex();

            if (requiresScopes)
                builder.AddOp(new ScopeEnd());
        }
    }
}
