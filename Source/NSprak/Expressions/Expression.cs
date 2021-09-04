using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Messaging;
using NSprak.Operations;
using NSprak.Tokens;

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

        public abstract IEnumerable<Expression> GetSubExpressions();

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

        public IEnumerable<Expression> GetSubExpressionDeep()
        {
            foreach (Expression child in GetSubExpressions())
            {
                yield return child;

                foreach (Expression descendent in child.GetSubExpressionDeep())
                    yield return descendent;
            }
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
    }
}
