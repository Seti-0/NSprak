using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Functions;
using NSprak.Functions.Signatures;
using NSprak.Language.Values;

namespace NSprak.Language.Libraries
{
    public static class CoreOperators
    {
        // Many of these could be op codes, directly
        // Also, there so much overlap - some way of doing generics or templates would be lovely

        /*
            As a reminder, these are the operator names:
        
            Add
            Subtract
            Multiply
            Divide

            Increment
            Decrement

            Set
            SelfPlus
            SelfLess
            SelfTimes
            SelfOver

            Not
            And
            Or

            EqualTo
            NotEqualTo

            LessThan
            LessThanOrEquals
            GreaterThan
            GreaterThanOrEquals

        */

        /*

            Arithmetic (Numbers and strings)
        
        */

        [SprakOperator(Operator.Names.Add)]
        public static SprakNumber Add(SprakNumber left, SprakNumber right)
        {
            return new SprakNumber(left.Value + right.Value);
        }

        [SprakOperator(Operator.Names.Subtract)]
        public static SprakNumber Subtract(SprakNumber left, SprakNumber right)
        {
            return new SprakNumber(left.Value - right.Value);
        }

        [SprakOperator(Operator.Names.Multiply)]
        public static SprakNumber Multiply(SprakNumber left, SprakNumber right)
        {
            return new SprakNumber(left.Value * right.Value);
        }

        [SprakOperator(Operator.Names.Divide)]
        public static SprakNumber Divide(SprakNumber left, SprakNumber right)
        {
            return new SprakNumber(left.Value / right.Value);
        }

        [SprakOperator(Operator.Names.Increment, InputSides.Left)]
        public static SprakNumber Increment(SprakNumber left)
        {
            return new SprakNumber(left.Value + 1);
        }

        [SprakOperator(Operator.Names.Decrement, InputSides.Left)]
        public static SprakNumber Decrement(SprakNumber left)
        {
            return new SprakNumber(left.Value - 1);
        }

        [SprakOperator(Operator.Names.Add)]
        public static SprakString Add(SprakString left, SprakString right)
        {
            return new SprakString(left.Value + right.Value);
        }

        [SprakOperator(Operator.Names.Subtract, InputSides.Right)]
        public static SprakNumber Add(SprakNumber right)
        {
            return new SprakNumber(-right.Value);
        }

        /*

            Boolean ops - Not, And, Or
        
        */

        [SprakOperator(Operator.Names.Not, InputSides.Right)]
        public static SprakBoolean Not(SprakBoolean right)
        {
            return new SprakBoolean(!right.Value);
        }

        [SprakOperator(Operator.Names.And)]
        public static SprakBoolean And(SprakBoolean left, SprakBoolean right)
        {
            return new SprakBoolean(left.Value && right.Value);
        }

        [SprakOperator(Operator.Names.Or)]
        public static SprakBoolean Or(SprakBoolean left, SprakBoolean right)
        {
            return new SprakBoolean(left.Value || right.Value);
        }

        /*

            General Comparison - Equals

        */

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakBoolean left, SprakBoolean right)
        {
            return new SprakBoolean(left.Value == right.Value);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left == right);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakString left, SprakString right)
        {
            return new SprakBoolean(left == right);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakArray left, SprakArray right)
        {
            return new SprakBoolean(left == right);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakConnection left, SprakConnection right)
        {
            return new SprakBoolean(left == right);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(Value left, Value right)
        {
            return new SprakBoolean(left == right);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakBoolean left, SprakBoolean right)
        {
            return new SprakBoolean(left != right);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left != right);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakString left, SprakString right)
        {
            return new SprakBoolean(left != right);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakArray left, SprakArray right)
        {
            return new SprakBoolean(left != right);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakConnection left, SprakConnection right)
        {
            return new SprakBoolean(left.ConnectionString != right.ConnectionString);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(Value left, Value right)
        {
            return new SprakBoolean(left != right);
        }

        /*

            Numeric Comparison - GreatherThan, LessThan, etc

        */

        [SprakOperator(Operator.Names.GreaterThan)]
        public static SprakBoolean GreaterThan(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left.Value > right.Value);
        }

        [SprakOperator(Operator.Names.LessThan)]
        public static SprakBoolean LessThan(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left.Value < right.Value);
        }

        [SprakOperator(Operator.Names.GreaterThanOrEquals)]
        public static SprakBoolean GreaterThanOrEquals(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left.Value >= right.Value);
        }

        [SprakOperator(Operator.Names.LessThanOrEquals)]
        public static SprakBoolean LessThanOrEquals(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left.Value <= right.Value);
        }
    }
}
