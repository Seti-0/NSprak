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
                builder.AddOp(new ScopeBegin(), header.IfToken);

            string currentEndLabel = builder.DeclareLabel("endIf");

            string finalEndLabel = null;
            if (header.NextConditionalComponentHint != null)
                finalEndLabel = builder.DeclareLabel("endConditional");

            builder.AddCode(header.Condition);

            builder.AddOp(new JumpLabelConditionalNegated(currentEndLabel), header.IfToken);

            foreach (Expression statement in header.ParentBlockHint.Statements)
                builder.AddCode(statement);

            if (header.NextConditionalComponentHint != null)
            {
                IConditionalSubComponent current = header.NextConditionalComponentHint;
                while (current != null)
                {
                    current.EndLabelHint = finalEndLabel;
                    current = current.NextConditionalComponentHint;
                }

                builder.AddOp(new JumpLabel(finalEndLabel));
            }

            builder.SetLabelToNext(currentEndLabel);

            if (header.NextConditionalComponentHint == null 
                && header.RequiresScopeHint)
            {
                builder.AddOp(new ScopeEnd(), header.ParentBlockHint.EndToken);
            }
        }

        public static void GenerateCode(ElseIfHeader header, GeneratorContext builder)
        {
            string currentEndLabel = builder.DeclareLabel("endElif");

            builder.AddCode(header.Condition);
            builder.AddOp(new JumpLabelConditionalNegated(currentEndLabel), header.IfToken);

            foreach (Expression statement in header.ParentBlockHint.Statements)
                builder.AddCode(statement);

            string finalEndLabel = header.EndLabelHint;
            if (finalEndLabel == null)
                throw new Exception("Encountered ElseIf Header without an end label!");

            if (header.NextConditionalComponentHint == null)
            {
                builder.SetLabelToNext(finalEndLabel);
                
                if (header.RequiresScopeHint)
                    builder.AddOp(new ScopeEnd(), header.ParentBlockHint.EndToken);
            }
            else
                builder.AddOp(new JumpLabel(finalEndLabel));

            builder.SetLabelToNext(currentEndLabel);
        }

        public static void GenerateCode(ElseHeader header, GeneratorContext builder)
        {
            foreach (Expression statement in header.ParentBlockHint.Statements)
                builder.AddCode(statement);

            string conditionalEndLabel = header.EndLabelHint;
            if (conditionalEndLabel == null)
                throw new Exception("Encountered Else Header without an end label!");

            builder.SetLabelToNext(conditionalEndLabel);
                
            if (header.RequiresScopeHint)
                builder.AddOp(new ScopeEnd(), header.ParentBlockHint.EndToken);
        }

        public static void GenerateCode(LoopHeader header, GeneratorContext builder)
        {
            bool requiresOuterScope = !header.IsInfinite;
            bool requiresInnerScope = header.RequiresScopeHint;

            if (requiresOuterScope)
                builder.AddOp(new ScopeBegin(), header.LoopToken);

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
                    builder.AddOp(new VariableCreate(endName), header.LoopToken);

                    endOp = new VariableGet(endName);
                }

                builder.AddCode(header.RangeStart);
                builder.AddOp(new VariableCreate(indexName), header.NameToken);

                builder.SetLabelToNext(continueLabel);

                builder.AddOp(new VariableGet(indexName), header.NameToken);
                builder.AddOp(endOp);
                builder.AddOp(new GreaterThan(), header.FromToken);
                builder.AddOp(new JumpLabelConditional(endLabel), header.FromToken);
            }
            else
            {
                builder.PushIndex();
                indexName = builder.GetIndexedName("index");
                string arrayName = builder.GetIndexedName("array");
                string countName = builder.GetIndexedName("count");
                string currentName = header.Name ?? "@";

                builder.AddCode(header.Array);
                builder.AddOp(new VariableCreate(arrayName), header.LoopToken);

                builder.AddOp(new ArrayCount(arrayName), header.LoopToken);
                builder.AddOp(new VariableCreate(countName), header.LoopToken);

                builder.AddOp(new LiteralValue(new SprakNumber(0)), header.LoopToken);
                builder.AddOp(new VariableCreate(indexName), header.LoopToken);

                builder.AddOp(new LiteralValue(SprakUnit.Value), header.LoopToken);
                builder.AddOp(new VariableCreate(currentName), header.LoopToken);

                builder.SetLabelToNext(continueLabel);

                builder.AddOp(new VariableGet(indexName), header.LoopToken);
                builder.AddOp(new VariableGet(countName), header.LoopToken);
                builder.AddOp(new GreaterThanOrEqualTo(), header.LoopToken);
                builder.AddOp(new JumpLabelConditional(endLabel), header.LoopToken);

                builder.AddOp(new VariableGet(arrayName), header.InToken);
                builder.AddOp(new VariableGet(indexName), header.InToken);
                builder.AddOp(new ArrayElementGet(), header.InToken);
                builder.AddOp(new VariableSet(currentName), header.InToken);

                builder.AddOp(new Increment(indexName), header.InToken);
            };

            // This is used by the continue command, currently.
            header.IndexNameHint = indexName;

            if (requiresInnerScope)
                builder.AddOp(new ScopeBegin(), header.LoopToken);

            foreach (Expression statement in header.ParentBlockHint.Statements)
                builder.AddCode(statement);

            if (requiresInnerScope)
                builder.AddOp(new ScopeEnd(), header.EndToken);

            if (header.IsRange)
                builder.AddOp(new Increment(indexName), header.EndToken);

            builder.AddOp(new JumpLabel(continueLabel), header.EndToken);
            builder.SetLabelToNext(endLabel);

            if (!header.IsInfinite)
                builder.PopIndex();

            if (requiresOuterScope)
                builder.AddOp(new ScopeEnd(), header.EndToken);

            builder.BreakLabels.Pop();
            builder.ContinueLabels.Pop();
        }
    }
}
