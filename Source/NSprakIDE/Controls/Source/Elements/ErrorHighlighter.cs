using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Messaging;
using NSprak.Tokens;

namespace NSprakIDE.Controls.Source
{
    public class ErrorHighlighter : IColorizerElement<Token>
    {
        public Messenger Messenger { get; set; }

        public ErrorHighlighter(Messenger messenger)
        {
            Messenger = messenger;
        }

        public bool CanApply(Token item) => true;

        public void Apply(VisualLineElement element, Token item)
        {
            if (Messenger == null)
                return;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 100, 100));
            var pen = new Pen(brush, 1);

            var decoration = new TextDecoration()
            {
                Location = TextDecorationLocation.Underline,
                Pen = pen,
                PenThicknessUnit = TextDecorationUnit.FontRecommended
            };

            foreach (Message message in Messenger.Messages) 
                if (message.Location != null 
                    && message.Location.Start < item.End 
                    && message.Location.End > item.Start)
                {
                    element.TextRunProperties.SetTextDecorations(new TextDecorationCollection());
                    element.TextRunProperties.TextDecorations.Add(decoration);
                }
        }
    }
}
