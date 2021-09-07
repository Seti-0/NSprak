using System;
using System.Collections.Generic;
using System.Text;

using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Expressions;

namespace NSprakIDE.Controls.Source
{
    public class ExecutionHighlighting : IColorizerElement<Expression>
    {
        public bool CanApply(Expression item) => false;

        public void Apply(VisualLineElement element, Expression item)
        {
        }
    }
}
