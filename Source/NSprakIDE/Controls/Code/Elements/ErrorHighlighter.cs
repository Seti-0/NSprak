using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Messaging;
using NSprak.Tokens;

namespace NSprakIDE.Controls.Code
{
    public class ErrorHighlighter : IColorizerElement<Token>
    {
        public MessageCollection Messages { get; set; }

        public void Apply(VisualLineElement element, Token item)
        {
            if (Messages == null)
                return;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(100, 10, 10));
            var pen = new Pen(brush, 2);

            var decoration = new TextDecoration()
            {
                Location = TextDecorationLocation.Underline,
                Pen = pen,
                PenThicknessUnit = TextDecorationUnit.FontRecommended
            };

            foreach (Message message in Messages.Messages) 
                if (message.Start < item.End && message.End > item.Start)
                    element.TextRunProperties.TextDecorations.Add(decoration);
        }
    }
}
