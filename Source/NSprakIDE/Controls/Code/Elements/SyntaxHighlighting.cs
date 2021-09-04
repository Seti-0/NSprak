using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Tokens;

using NSprakIDE.Themes;

namespace NSprakIDE.Controls.Code
{
    public class SyntaxHighlighting : IColorizerElement<Token>
    {
        public bool CanApply(Token item) => true;

        public void Apply(VisualLineElement element, Token token)
        {
            string key = token.Type switch
            {
                TokenType.Comment => Theme.Source.Comment,
                TokenType.Boolean => Theme.Source.Boolean,
                TokenType.KeyWord => Theme.Source.Keyword,
                TokenType.KeySymbol => Theme.Source.KeySymbol,
                TokenType.Number => Theme.Source.Number,
                TokenType.Operator => Theme.Source.Operator,
                TokenType.String => Theme.Source.String,
                TokenType.Type => Theme.Source.Type,

                TokenType.Name => 
                    (token.Hints & TokenHints.Function) != 0 ?
                        (token.Hints & TokenHints.BuiltInFunction) != 0 ?
                            Theme.Source.LibraryFunction 
                            : Theme.Source.UserFunction 
                            : Theme.Source.Name,

                _ => Theme.Source.Error
            };

            Brush brush = Theme.Get(key);
            element.TextRunProperties.SetForegroundBrush(brush);

            if (token.Type == TokenType.Comment)
            {
                Brush background = Theme.Get(Theme.Source.CommentBackground);
                element.BackgroundBrush = background;
            }
        }
    }
}
