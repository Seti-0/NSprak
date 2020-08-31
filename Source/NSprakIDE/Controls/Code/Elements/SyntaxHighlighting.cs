using ICSharpCode.AvalonEdit.Rendering;
using NSprak.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NSprakIDE.Controls.Code
{
    public class SyntaxHighlighterTheme
    {
        public const string
            Name = "SyntaxColors_Name",
            Keyword = "SyntaxColors_Keyword",
            KeySymbol = "SyntaxColors_KeySymbol",
            Type = "SyntaxColors_Type",
            Operator = "SyntaxColors_Operator",
            String = "SyntaxColors_String",
            Boolean = "SyntaxColors_Boolean",
            Number = "SyntaxColors_Number",
            Comment = "SyntaxColors_Comment",
            Error = "SyntaxColors_Error",

            UserFunction = "SyntaxColors_UserFunction",
            LibraryFunction = "SyntaxColors_LibraryFunction",
            
            CommentBackground = "SynaxColors_CommentBackground";


        private Brush _name, _keyword, _keySymbol, 
            _type, _operator, _string, _boolean, 
            _number, _comment, _error, _commentBg,
            _function;

        public SyntaxHighlighterTheme(Func<string, Brush> resourceFinder)
        {
            _error = resourceFinder(Error) ?? new SolidColorBrush(Colors.Black);

            _name = resourceFinder(Name);
            _keyword = resourceFinder(Keyword);
            _keySymbol = resourceFinder(KeySymbol);
            _type = resourceFinder(Type);
            _operator = resourceFinder(Operator);
            _string = resourceFinder(String);
            _boolean = resourceFinder(Boolean);
            _number = resourceFinder(Number);
            _comment = resourceFinder(Comment);
            _commentBg = resourceFinder(CommentBackground);
            _function = resourceFinder(UserFunction);
        }

        public Brush GetCommentBackground()
        {
            return _commentBg;
        }

        public Brush GetColor(TokenType type, TokenHints hints)
        {
            Brush result = type switch
            {
                TokenType.KeyWord => _keyword,
                TokenType.KeySymbol => _keySymbol,
                TokenType.Type => _type,
                TokenType.Operator => _operator,
                TokenType.String => _string,
                TokenType.Boolean => _boolean,
                TokenType.Number => _number,
                TokenType.Comment => _comment,

                TokenType.Name => 
                    (hints & TokenHints.Function) != 0 ? _function : _name,

                _ => _error
            };

            return result ?? _error;
        }
    }

    public class SyntaxHighlighting : IColorizerElement<Token>
    {
        SyntaxHighlighterTheme _theme;

        public SyntaxHighlighting(SyntaxHighlighterTheme theme)
        {
            _theme = theme;
        }

        public void Apply(VisualLineElement element, Token token)
        {
            /*
            var brush = new SolidColorBrush(Color.FromRgb(100, 10, 255));
            var pen = new Pen(brush, 2);

            var decoration = new TextDecoration()
            {
                Location = TextDecorationLocation.Underline,
                Pen = pen,
                PenThicknessUnit = TextDecorationUnit.FontRecommended
            };

            var collection = new TextDecorationCollection();
            collection.Add(decoration);

            element.TextRunProperties.SetTextDecorations(collection);
            */


            element.TextRunProperties.SetForegroundBrush(_theme.GetColor(token.Type, token.Hints));

            if (token.Type == TokenType.Comment)
                element.BackgroundBrush = _theme.GetCommentBackground();
        }
    }
}
