using ICSharpCode.AvalonEdit.Rendering;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Media;

namespace NSprakIDE.Controls.Source
{
    public class TestExpressions : IColorizerElement<Expression>
    {
        public bool CanApply(Expression item) => true;

        public void Apply(VisualLineElement element, Expression item)
        {            
            element.BackgroundBrush = new SolidColorBrush(GetColor(item));
        }

        private Color GetColor(Expression item)
        {
            return item switch
            {
                Block block => GetColor(block.Header),
                Command _ => Colors.Blue,
                FunctionCall _ => Colors.Orange,
                FunctionHeader _ => Colors.Blue,
                IfHeader _ => Colors.DarkRed,
                LiteralArrayGet _ => Colors.Yellow,
                LiteralGet _ => Colors.Yellow,
                LoopHeader _ => Colors.Red,
                MainHeader _ => Colors.Orange,
                OperatorCall _ => Colors.Green,
                Return _ => Colors.Blue,
                VariableAssignment _ => Colors.Orange,
                VariableReference _ => Colors.Yellow,
                _ => Colors.Black
            };
        }
    }
}
