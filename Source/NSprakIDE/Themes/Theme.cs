using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NSprakIDE.Themes
{
    public static class Theme
    {
        public static class General
        {
            public const string
                Accent = "NSprakIDE.Accent",
                Background = "NSprakIDE.Background",
                Background2 = "NSprakIDE.Background2",
                Toolbar = "NSprakIDE.Toolbar";
        }

        public static class Source
        {
            public const string
                Boolean = "NSprakIDE.Source.Boolean",
                Comment = "NSprakIDE.Source.Comment",
                DiffChange = "NSprakIDE.Source.DiffChange",
                DiffInsert = "NSprakIDE.Source.DiffInsert",
                DiffRemove = "NSprakIDE.Source.DiffRemove",
                Error = "NSprakIDE.Source.Error",
                Keyword = "NSprakIDE.Source.Keyword",
                KeySymbol = "NSprakIDE.Source.KeySymbol",
                LibraryFunction = "NSprakIDE.Source.LibraryFunction",
                Name = "NSprakIDE.Source.Name",
                Number = "NSprakIDE.Source.Number",
                Operator = "NSprakIDE.Source.Operator",
                String = "NSprakIDE.Source.String",
                Type = "NSprakIDE.Source.Type",
                UserFunction = "NSprakIDE.Source.UserFunction",
                CommentBackground = "NSprakIDE.Source.CommentBackground";
        }

        public static class Runtime
        {
            public const string
                Next = "NSprakIDE.Runtime.Next",
                NextText = "NSprakIDE.Runtime.NextText",
                Breakpoint = "NSprakIDE.Runtime.Breakpoint",
                BreakpointText = "NSprakIDE.Runtime.BreakpointText";
        }

        public static class Expressions
        {
            public const string
                Comment = "NSprakIDE.Expressions.Comment",
                Debug = "NSprakIDE.Expressions.Debug",
                KeySymbol = "NSprakIDE.Expressions.KeySymbol",
                Keyword = "NSprakIDE.Expressions.Keyword",
                Literal = "NSprakIDE.Expressions.Literal",
                Name = "NSprakIDE.Expressions.Name",
                Operator = "NSprakIDE.Expressions.Operator",
                Type = "NSprakIDE.Expressions.Type";
        }

        public static class Operations
        {
            public const string
                Default = "NSprakIDE.Operations.Default",
                Background = "NSprakIDE.Operations.Background",
                BackgroundAlternate = "NSprakIDE.Operations.BackgroundAlternate",

                LineNumberText = "NSprakIDE.Operations.LineNumberText",

                Param = "NSprakIDE.Operations.Param",
                NumberParam = "NSprakIDE.Operations.NumberParam",
                NameParam = "NSprakIDE.Operations.NameParam",
                ValueParam = "NSprakIDE.Operations.ValueParam",

                Error = "NSprakIDE.Operations.Error",
                Comment = "NSprakIDE.Operations.Comment",
                Label = "NSprakIDE.Operations.Label",

                Next = "NSprakIDE.Runtime.Next",
                NextText = "NSprakIDE.Runtime.NextText",
                Breakpoint = "NSprakIDE.Runtime.Breakpoint",
                BreakpointText = "NSprakIDE.Runtime.BreakpointText";
        }

        public static class Screen
        {
            public const string
                Border = "NSprakIDE.Screen.Border",
                Background = "NSprakIDE.Screen.Background",
                Text = "NSprakIDE.Screen.Text";
        }

        private static readonly Dictionary<string, Brush> _brushes 
            = new Dictionary<string, Brush>();

        public static Brush GetBrush(string name)
        {
            // This cache might be needed, depending on how FindResource works.
            // But this method is called many times during syntax highlighting,
            // so better safe than sorry.
            if (!_brushes.TryGetValue(name, out Brush brush))
            {
                brush = (Brush)Application.Current.FindResource(name);
                _brushes.Add(name, brush);
            }

            return brush;
        }
    }
}
