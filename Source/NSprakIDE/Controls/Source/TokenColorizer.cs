using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using NSprak.Tokens;

namespace NSprakIDE.Controls.Source
{
    public class TokenColorizer : ColorizingTransformer
    {
        public TokenPage Tokens { get; set; }

        public IEnumerable<IColorizerElement<Token>> Elements { get; set; }

        protected override void Colorize(ITextRunConstructionContext context)
        {
            if (Tokens == null)
                return;

            if (Elements == null)
                return;

            int globalStart = context.VisualLine.FirstDocumentLine.Offset;
            int globalEnd = context.VisualLine.LastDocumentLine.EndOffset;
            int firstLineNumber = context.VisualLine.FirstDocumentLine.LineNumber;

            TokenSeeker enumerator = new TokenSeeker(Tokens);

            enumerator.SeekLine(firstLineNumber - 1);
            enumerator.EnsureStarted();

            bool hasNext = enumerator.HasCurrent;

            while (hasNext)
            {
                hasNext = ApplyOverlap(enumerator.Current, globalStart, globalEnd, context.VisualLine);
                hasNext &= enumerator.Step();
            }
        }

        private bool ApplyOverlap(Token token, int start, int end, VisualLine visualLine)
        {
            if (token.Start >= end)
                return false;

            if (token.End <= start)
                return true;

            int overlapStart = Math.Max(start, token.Start);
            int overlapEnd = Math.Min(end, token.End);

            int offset = visualLine.FirstDocumentLine.Offset;
            overlapStart = visualLine.GetVisualColumn(overlapStart - offset);
            overlapEnd = visualLine.GetVisualColumn(overlapEnd - offset);

            void ApplyColorizerElements(VisualLineElement lineElement)
            {
                foreach (IColorizerElement<Token> colorizerElement in Elements)
                    colorizerElement.Apply(lineElement, token);
            }

            ChangeVisualElements(overlapStart, overlapEnd, ApplyColorizerElements);

            return true;
        }
    }
}
