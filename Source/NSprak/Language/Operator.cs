using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Exceptions;
using NSprak.Tokens;

namespace NSprak.Language
{
    public enum OperatorSide
    {
        Left, Right, Both, Neither
    }

    public static class OperatorSideExtMethods
    {
        public static bool None(this OperatorSide side)
        {
            return side == OperatorSide.Neither;
        }

        public static bool Single(this OperatorSide side)
        {
            return side == OperatorSide.Left || side == OperatorSide.Right;
        }

        public static bool IsBinary(this OperatorSide side)
        {
            return side == OperatorSide.Both;
        }
    }

    public partial class Operator
    {
        public string Text { get; }

        public string Name { get; }

        public OperatorSide Syntax { get; private set; } = OperatorSide.Neither;

        public OperatorSide Inputs { get; private set; } = OperatorSide.Neither;

        public OperatorSide Assignments { get; private set; } = OperatorSide.Neither;

        public bool IsAssignment => !Assignments.None();


        public Operator(string name, string text)
        {
            Text = text;
            Name = name;
        }
    }

    public partial class Operator
    {
        public static class Names
        {
            public const string

               Add = "Add",
               Subtract = "Subtract",
               Multiply = "Multiply",
               Divide = "Divide",

               Increment = "Increment",
               Decrement = "Decrement",

               Set = "Set",
               SelfPlus = "SelfPlus",
               SelfLess = "SelfLess",
               SelfTimes = "SelfTimes",
               SelfOver = "SelfOver",

               Not = "Not",
               EqualTo = "EqualTo",
               NotEqualTo = "NotEqualTo",

               LessThan = "LessThan",
               LessThanOrEquals = "LessThanOrEquals",
               GreaterThan = "GreaterThan",
               GreaterThanOrEquals = "GreaterThanOrEquals";
        }

        public static readonly Operator

               Add = Binary(Names.Add, "+"),
               Subtract = Binary(Names.Subtract, "-"),
               Multiply = Binary(Names.Multiply, "*"),
               Divide = Binary(Names.Divide, "/"),

               Increment = NullaryAssignment(Names.Increment, "++"),
               Decrement = NullaryAssignment(Names.Decrement, "--"),

               Set = UnaryAssignment(Names.Set, "="),

               SelfPlus = BinaryAssignment(Names.SelfPlus, "+="),
               SelfLess = BinaryAssignment(Names.SelfLess, "-="),
               SelfTimes = BinaryAssignment(Names.SelfTimes, "*="),
               SelfOver = BinaryAssignment(Names.SelfOver, "/="),

               Not = Right(Names.Not, "!"),
               EqualTo = Binary(Names.EqualTo, "=="),
               NotEqualTo = Binary(Names.NotEqualTo, "!="),

               LessThan = Binary(Names.LessThan, "<"),
               LessThanOrEquals = Binary(Names.LessThanOrEquals, "<="),
               GreaterThan = Binary(Names.GreaterThan, ">"),
               GreaterThanOrEquals = Binary(Names.GreaterThanOrEquals, ">=");

        public static readonly IReadOnlyCollection<Operator> All;

        private readonly static Dictionary<string, Operator> operatorsByName;
        private readonly static Dictionary<string, Operator> operatorsByText;

        static Operator()
        {
            All = new Operator[]
            {
                Add, Subtract, Multiply, Divide,
                Increment, Decrement,
                Set, SelfPlus, SelfLess, SelfTimes, SelfOver,
                Not, EqualTo, NotEqualTo,
                LessThan, LessThanOrEquals, GreaterThan, GreaterThanOrEquals
            };

            operatorsByName = All.ToDictionary(x => x.Name);
            operatorsByText = All.ToDictionary(x => x.Text);
        }

        public static bool IsOperator(string name = null, string text = null)
        {
            if (name != null)
                return operatorsByName.ContainsKey(name);

            if (text != null)
                return operatorsByText.ContainsKey(text);

            return false;
        }

        public static bool TryParse(out Operator op, string name = null, string text = null)
        {
            if (name != null)
                return operatorsByName.TryGetValue(name, out op);

            if (text != null)
                return operatorsByText.TryGetValue(text, out op);

            op = null;
            return false;
        }

        private static Operator Right(string name, string text)
        {
            return new Operator(name, text)
            {
                Syntax = OperatorSide.Right,
                Inputs = OperatorSide.Right,
                Assignments = OperatorSide.Neither
            };
        }

        private static Operator Binary(string name, string text)
        {
            return new Operator(name, text)
            {
                Syntax = OperatorSide.Both,
                Inputs = OperatorSide.Both,
                Assignments = OperatorSide.Neither
            };
        }

        private static Operator BinaryAssignment(string name, string text)
        {
            return new Operator(name, text)
            {
                Syntax = OperatorSide.Both,
                Inputs = OperatorSide.Both,
                Assignments = OperatorSide.Left
            };
        }

        private static Operator UnaryAssignment(string name, string text)
        {
            return new Operator(name, text)
            {
                Syntax = OperatorSide.Both,
                Inputs = OperatorSide.Right,
                Assignments = OperatorSide.Left
            };
        }

        private static Operator NullaryAssignment(string name, string text)
        {
            return new Operator(name, text)
            {
                Syntax = OperatorSide.Left,
                Inputs = OperatorSide.Left,
                Assignments = OperatorSide.Left
            };
        }
    }
}
