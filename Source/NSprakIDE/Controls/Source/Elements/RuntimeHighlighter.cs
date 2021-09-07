using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;

using NSprak.Execution;
using NSprak.Expressions;
using NSprak.Operations;

using NSprakIDE.Themes;

using ICSharpCode.AvalonEdit.Rendering;

namespace NSprakIDE.Controls.Source
{
    public class RuntimeHighlighter : IColorizerElement<Expression>
    {
        public Executor Executor { get; set; }

        private bool TryGetKeys(Expression item, 
            out string background, out string text)
        {
            background = null;
            text = null;
            bool found = false;

            foreach (OpDebugInfo op in item.OperatorsHint)
            {
                if (Executor != null && op.Index == Executor.Instructions.Index)
                {
                    background = Theme.Runtime.Next;
                    text = Theme.Runtime.NextText;
                    found = true;
                    break;
                }
                else if (op.Breakpoint)
                {
                    background = Theme.Runtime.Breakpoint;
                    text = Theme.Runtime.BreakpointText;
                    found = true;
                    // Don't break, in case any remaining tokens 
                    // are to be highlighted next, which takes precendence
                    // over highlighting breakpoints.
                }
            }

            return found;
        }

        public bool CanApply(Expression item)
        {
            return TryGetKeys(item, out _, out _);
        }

        public void Apply(VisualLineElement element, Expression item)
        {
            if (!TryGetKeys(item, out string backgroundKey, out string textKey))
                return;

            element.BackgroundBrush = Theme.GetBrush(backgroundKey);
            element.TextRunProperties.SetForegroundBrush(Theme.GetBrush(textKey));
        }
    }
}
