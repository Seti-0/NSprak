using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Messaging;
using NSprak.Operations;
using NSprak.Tokens;
using NSprak.Tests;

namespace NSprak.Expressions
{
    public abstract class Expression
    {
        public abstract Token StartToken { get; }

        public abstract Token EndToken { get; }

        public Expression ParentHint { get; set; } = null;

        public Block ParentBlockHint { get; set; } = null;

        public SprakType TypeHint { get; set; } = SprakType.Unit;

        public List<OpDebugInfo> OperatorsHint { get; } = new List<OpDebugInfo>();

        public List<TestCommand> TestsHint { get; private set; } = null;

        public abstract IEnumerable<Expression> GetSubExpressions();

        public abstract IEnumerable<Token> GetTokens();

        public bool IsDescendantOf(Expression other)
        {
            Expression ancestor = ParentHint;
            while (ancestor != null)
            {
                if (ancestor == other)
                    return true;

                ancestor = ancestor.ParentHint;
            }

            return false;

        }

        public Expression GetNextSibling()
        {
            if (ParentHint == null) 
                return null;

            IEnumerator<Expression> siblings = ParentHint
                .GetSubExpressions()
                .GetEnumerator();

            while (siblings.MoveNext())
                if (siblings.Current == this)
                {
                    if (siblings.MoveNext())
                        return siblings.Current;
                    break;
                }

            return null;
        }

        public virtual string GetTraceString()
        {
            Block ancestor = ParentBlockHint;
            FunctionHeader function = null;

            while (ancestor != null && function == null)
            {
                function = ancestor.Header as FunctionHeader;
                ancestor = ancestor.ParentBlockHint;
            }

            string result = ToString();

            if (function != null && function != this)
                result = $"{function.Signature.FullName}: {result}";

            string line = StartToken?.Line.LineNumber.ToString();

            if (line != null)
                result = $"[line {line}] {result}";

            return result;
        }

        public void AddTest(TestCommand command)
        {
            if (TestsHint == null)
                TestsHint = new List<TestCommand>();

            TestsHint.Add(command);
        }
    }
}
