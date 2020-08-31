using System;
using System.Collections.Generic;
using System.Text;

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
            EqualTo
            NotEqualTo

            LessThan
            LessThanOrEquals
            GreaterThan
            GreaterThanOrEquals

        */

        #region Numeric Arithmetic
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

        [SprakOperator(Operator.Names.SelfPlus)]
        public static SprakNumber SelfPlus(SprakNumber left, SprakNumber right)
        {
            return Add(left, right);
        }

        [SprakOperator(Operator.Names.SelfLess)]
        public static SprakNumber SelfLess(SprakNumber left, SprakNumber right)
        {
            return Subtract(left, right);
        }

        [SprakOperator(Operator.Names.SelfTimes)]
        public static SprakNumber SelfTimes(SprakNumber left, SprakNumber right)
        {
            return Multiply(left, right);
        }

        [SprakOperator(Operator.Names.SelfOver)]
        public static SprakNumber SelfOver(SprakNumber left, SprakNumber right)
        {
            return Divide(left, right);
        }

        [SprakOperator(Operator.Names.Increment)]
        public static SprakNumber Increment(SprakNumber left)
        {
            return new SprakNumber(left.Value + 1);
        }

        [SprakOperator(Operator.Names.Decrement)]
        public static SprakNumber Decrement(SprakNumber left)
        {
            return new SprakNumber(left.Value - 1);
        }
        #endregion

        #region String Arithmetic
        [SprakOperator(Operator.Names.Add)]
        public static SprakString Add(SprakString left, SprakString right)
        {
            return new SprakString(left.Value + right.Value);
        }

        [SprakOperator(Operator.Names.SelfPlus)]
        public static SprakString SelfPlus(SprakString left, SprakString right)
        {
            return Add(left, right);
        }
        #endregion

        #region Set
        [SprakOperator(Operator.Names.Set)]
        public static SprakBoolean Set(SprakBoolean right)
        {
            return right;
        }

        [SprakOperator(Operator.Names.Set)]
        public static SprakNumber Set(SprakNumber right)
        {
            return right;
        }

        [SprakOperator(Operator.Names.Set)]
        public static SprakString Set(SprakString right)
        {
            return right;
        }

        [SprakOperator(Operator.Names.Set)]
        public static SprakArray Set(SprakArray right)
        {
            return right;
        }

        [SprakOperator(Operator.Names.Set)]
        public static SprakConnection Set(SprakConnection right)
        {
            return right;
        }

        [SprakOperator(Operator.Names.Set)]
        public static Value Set(Value right)
        {
            return right;
        }
        #endregion

        #region Boolean
        [SprakOperator(Operator.Names.Not)]
        public static SprakBoolean Not(SprakBoolean right)
        {
            return new SprakBoolean(!right.Value);
        }
        #endregion

        #region General Comparison
        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakBoolean left, SprakBoolean right)
        {
            return new SprakBoolean(left.Value == right.Value);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left.Value == right.Value);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakString left, SprakString right)
        {
            return new SprakBoolean(left.Value == right.Value);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakArray left, SprakArray right)
        {
            return new SprakBoolean(left.Value == right.Value);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(SprakConnection left, SprakConnection right)
        {
            return new SprakBoolean(left.ConnectionString == right.ConnectionString);
        }

        [SprakOperator(Operator.Names.EqualTo)]
        public static SprakBoolean Equal(Value left, Value right)
        {
            return new SprakBoolean(left == right);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakBoolean left, SprakBoolean right)
        {
            return new SprakBoolean(left.Value != right.Value);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakNumber left, SprakNumber right)
        {
            return new SprakBoolean(left.Value != right.Value);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakString left, SprakString right)
        {
            return new SprakBoolean(left.Value != right.Value);
        }

        [SprakOperator(Operator.Names.NotEqualTo)]
        public static SprakBoolean NotEqual(SprakArray left, SprakArray right)
        {
            return new SprakBoolean(left.Value != right.Value);
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
        #endregion

        #region Numeric Comparison
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
        #endregion
    }
}
