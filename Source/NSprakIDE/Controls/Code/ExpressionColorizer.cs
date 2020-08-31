using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Expressions;
using NSprak.Tokens;
using Expression = NSprak.Expressions.Expression;

namespace NSprakIDE.Controls.Code
{
    public class ExpressionColorizer : ColorizingTransformer
    {
        public ExpressionTree Tree { get; set; }

        public IEnumerable<IColorizerElement<Expression>> Elements { get; set; }

        protected override void Colorize(ITextRunConstructionContext context)
        {
            if (Tree == null)
                return;

            if (Elements == null)
                return;

            int startCharacter = context.VisualLine.FirstDocumentLine.Offset;
            int endCharacter = context.VisualLine.LastDocumentLine.EndOffset;

            foreach (Expression expression in FindExpressions(startCharacter, endCharacter))
                ApplyOverlap(expression, startCharacter, endCharacter, context.VisualLine);
        }

        private bool ApplyOverlap(Expression expression, int start, int end, VisualLine visualLine)
        {
            if (expression.StartToken.Start >= end)
                return false;

            if (expression.EndToken.End <= start)
                return true;

            int overlapStart = Math.Max(start, expression.StartToken.Start);
            int overlapEnd = Math.Min(end, expression.EndToken.End);

            int offset = visualLine.FirstDocumentLine.Offset;
            overlapStart = visualLine.GetVisualColumn(overlapStart - offset);
            overlapEnd = visualLine.GetVisualColumn(overlapEnd - offset);

            void ApplyColorizerElements(VisualLineElement lineElement)
            {
                foreach (IColorizerElement<Expression> colorizerElement in Elements)
                    colorizerElement.Apply(lineElement, expression);
            }

            ChangeVisualElements(overlapStart, overlapEnd, ApplyColorizerElements);

            return true;
        }

        private IEnumerable<Expression> FindExpressions(int rangeStart, int rangeEnd)
        {
            List<Expression> results = new List<Expression>();

            Queue<Expression> targets = new Queue<Expression>();

            foreach (Expression subExpression in Tree.Root.Statements)
                targets.Enqueue(subExpression);

            while (targets.Count > 0)
            {
                Expression current = targets.Dequeue();

                if (rangeStart >= current.EndToken.End)
                    continue;

                if (rangeEnd <= current.StartToken.Start)
                    continue;

                results.Add(current);

                foreach (Expression subExpression in current.GetSubExpressions())
                    targets.Enqueue(subExpression);
            }

            return results;
        }
    }
}
