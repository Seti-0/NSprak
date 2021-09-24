using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Exceptions;
using NSprak.Tokens;

namespace NSprak.Language
{
    public partial class Operator
    {
        public string Text { get; }

        public string Name { get; }

        public bool IsAssignment { get; }

        public string AssignmentOperation { get; }

        public Operator(string name, string text, 
            string assignmentOp = null, bool isAssignment = false)
        {
            Text = text;
            Name = name;
            AssignmentOperation = assignmentOp;
            IsAssignment = isAssignment || (assignmentOp != null);
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
               And = "And",
               Or = "Or",

               EqualTo = "EqualTo",
               NotEqualTo = "NotEqualTo",

               LessThan = "LessThan",
               LessThanOrEquals = "LessThanOrEquals",
               GreaterThan = "GreaterThan",
               GreaterThanOrEquals = "GreaterThanOrEquals";
        }

        public static readonly Operator

               Add = new Operator(Names.Add, "+"),
               Subtract = new Operator(Names.Subtract, "-"),
               Multiply = new Operator(Names.Multiply, "*"),
               Divide = new Operator(Names.Divide, "/"),

               Increment = new Operator(Names.Increment, "++", Names.Increment),
               Decrement = new Operator(Names.Decrement, "--", Names.Decrement),

               Set = new Operator(Names.Set, "=", isAssignment: true),

               SelfPlus = new Operator(Names.SelfPlus, "+=", Names.Add),
               SelfLess = new Operator(Names.SelfLess, "-=", Names.Subtract),
               SelfTimes = new Operator(Names.SelfTimes, "*=", Names.Multiply),
               SelfOver = new Operator(Names.SelfOver, "/=", Names.Divide),

               Not = new Operator(Names.Not, "!"),
               And = new Operator(Names.And, "and"),
               Or = new Operator(Names.Or, "or"),

               EqualTo = new Operator(Names.EqualTo, "=="),
               NotEqualTo = new Operator(Names.NotEqualTo, "!="),

               LessThan = new Operator(Names.LessThan, "<"),
               LessThanOrEquals = new Operator(Names.LessThanOrEquals, "<="),
               GreaterThan = new Operator(Names.GreaterThan, ">"),
               GreaterThanOrEquals = new Operator(Names.GreaterThanOrEquals, ">=");

        public static Operator[][] OperatorPrecedenceGroups = new Operator[][]
        {
            // Precedence only applies to binary non-assignment operators
            // Unary operators take precendence over all of these
            // Assignments happen after the rest of the expression is evaluated.

            new Operator[] { 
                Multiply, Divide
            },
            new Operator[]
            {
                Add, Subtract
            },
            new Operator[]
            {
                GreaterThan, LessThan,
                GreaterThanOrEquals, LessThanOrEquals
            },
            new Operator[]
            {
                EqualTo, NotEqualTo
            },
            new Operator[]
            {
                And, Or
            }
        };

        public static readonly IReadOnlyCollection<Operator> All;

        private readonly static Dictionary<string, Operator> operatorsByName;
        private readonly static Dictionary<string, Operator> operatorsByText;

        public static readonly IReadOnlyCollection<string> Keywords;

        static Operator()
        {
            All = new Operator[]
            {
                Add, Subtract, Multiply, Divide,
                Increment, Decrement,
                Set, SelfPlus, SelfLess, SelfTimes, SelfOver,
                Not, And, Or, EqualTo, NotEqualTo,
                LessThan, LessThanOrEquals, GreaterThan, GreaterThanOrEquals
            };

            operatorsByName = All.ToDictionary(x => x.Name);
            operatorsByText = All.ToDictionary(x => x.Text);

            Keywords = All
                .Select(x => x.Text)
                .Where(x => char.IsLetter(x[0]))
                .ToList();
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
    }
}
