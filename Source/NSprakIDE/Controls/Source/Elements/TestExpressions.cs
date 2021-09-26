using ICSharpCode.AvalonEdit.Rendering;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NSprakIDE.Controls.Source
{
    using SprakExpression = NSprak.Expressions.Expression;

    public class TestExpressions : IColorizerElement<SprakExpression>
    {
        public bool CanApply(SprakExpression item) => true;

        public void Apply(VisualLineElement element, SprakExpression item)
        {
            SolidColorBrush brush = new SolidColorBrush(GetColor(item));
            Pen pen = new Pen(brush, 1);

            TextDecoration decoration = new TextDecoration()
            {
                Location = TextDecorationLocation.Underline,
                Pen = pen,
                PenThicknessUnit = TextDecorationUnit.FontRecommended
            };

            element.TextRunProperties.SetTextDecorations(new TextDecorationCollection());
            element.TextRunProperties.TextDecorations.Add(decoration);
            element.BackgroundBrush = new SolidColorBrush(GetColor(item));
        }

        private Color GetColor(SprakExpression item)
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
