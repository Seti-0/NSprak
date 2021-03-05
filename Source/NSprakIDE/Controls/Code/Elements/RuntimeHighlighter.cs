using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;

using NSprak.Expressions;

using ICSharpCode.AvalonEdit.Rendering;

namespace NSprakIDE.Controls.Code
{
    public class RuntimeHighlighter : IColorizerElement<Expression>
    {
        private Func<string, Brush> _resourceFinder;

        public RuntimeHighlighter(Func<string, Brush> resourceFinder)
        {
            _resourceFinder = resourceFinder;
        }

        public void Apply(VisualLineElement element, Expression item)
        {
            Color? color = null;

            if (item.OperatorsHint.Any(x => x.Breakpoint))
                color = new Color();

            if (color.HasValue)
                element.BackgroundBrush = _resourceFinder(BrushNames.Breakpoint);
        }
    }
}
