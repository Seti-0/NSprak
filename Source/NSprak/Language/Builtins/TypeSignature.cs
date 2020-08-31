using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace NSprak.Language.Builtins
{
    public interface IMatchable<TSelf> where TSelf : IMatchable<TSelf>
    {
        public bool Matches(TSelf other);
    }

    public class FunctionTypeSignature : IEquatable<FunctionTypeSignature>
    {
        public IReadOnlyList<SprakType> Parameters { get; }

        public string UniqueID => string.Join(',', Parameters.Select(x => x.UniqueID));

        public FunctionTypeSignature(params SprakType[] parameters)
        {
            Parameters = parameters;
        }

        public FunctionTypeSignature(IReadOnlyList<SprakType> parameters)
        {
            Parameters = parameters;
        }

        public override string ToString()
        {
            return UniqueID;
        }

        public bool CanAccept(FunctionTypeSignature target, AssignmentResolver resolver)
        {
            if (target.Parameters.Count != Parameters.Count)
                return false;

            for (int i = 0; i < Parameters.Count; i++)
            {
                SprakType destination = Parameters[i];
                SprakType source = target.Parameters[i];

                if (!resolver.IsAssignable(source, destination))
                    return false;
            }

            return true;
        }

        #region Equals

        // While equals is certainly used, I doubt the hashcode will be

        public override int GetHashCode()
        {
            if (Parameters.Count == 0)
                return 0;

            int hash = Parameters.First().GetHashCode();

            for (int i = 1; i < Parameters.Count; i++)
                hash = HashCode.Combine(hash, Parameters[i]);

            return hash;
        }

        public bool Equals([AllowNull] FunctionTypeSignature other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            if (other.Parameters.Count != Parameters.Count)
                return false;

            for (int i = 0; i < other.Parameters.Count; i++)
                if (other.Parameters[i] != Parameters[i])
                    return false;

            return true;
        }

        public sealed override bool Equals(object obj)
        {
            if (obj is FunctionTypeSignature parameter)
                return Equals(parameter);

            return false;
        }

        public static bool operator ==(FunctionTypeSignature one, FunctionTypeSignature two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(FunctionTypeSignature one, FunctionTypeSignature two)
        {
            return !Equals(one, two);
        }
        #endregion
    }

    public class ConversionTypeSignature : IEquatable<ConversionTypeSignature>
    {
        public SprakType Input { get; }

        // Yes, the unique thing about a conversion is that the return value is a part of 
        // it's signature
        public SprakType Output { get; }

        public bool? IsPerfectHint { get; }

        public string UniqueID
        {
            get => $"{Input.UniqueID} -> {Output.UniqueID}";
        }

        public ConversionTypeSignature(SprakType input, SprakType output, bool? isPerfect = null)
        {
            Input = input;
            Output = output;
            IsPerfectHint = isPerfect;
        }

        public override string ToString()
        {
            return UniqueID;
        }

        public bool Matches(ConversionTypeSignature other)
        {
            return other.Output == Output
                && other.Input == Input;
        }

        #region Equals
        public override int GetHashCode()
        {
            return HashCode.Combine(Input, Output);
        }

        public bool Equals([AllowNull] ConversionTypeSignature other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            return other.Output == Output && other.Input == Input;
        }

        public sealed override bool Equals(object obj)
        {
            if (obj is ConversionTypeSignature parameter)
                return Equals(parameter);

            return false;
        }

        public static bool operator ==(ConversionTypeSignature one, ConversionTypeSignature two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(ConversionTypeSignature one, ConversionTypeSignature two)
        {
            return !Equals(one, two);
        }
        #endregion
    }

    public class OperatorTypeSignature : IEquatable<OperatorTypeSignature>
    {
        public SprakType LeftParam { get; }

        public SprakType RightParam { get; }

        public string UniqueID
        {
            get
            {
                if (LeftParam != null && RightParam != null)
                    return LeftParam.UniqueID + "," + RightParam.UniqueID;

                else return LeftParam?.UniqueID ?? RightParam?.UniqueID ?? "";
            }
        }

        public OperatorTypeSignature(SprakType left, SprakType right)
        {
            LeftParam = left;
            RightParam = right;
        }

        public override string ToString()
        {
            return UniqueID;
        }

        public bool CanAccept(OperatorTypeSignature other, AssignmentResolver resolver)
        {

            if (LeftParam == null)
            {
                if (other.LeftParam != null)
                    return false;
            }
            else
            {
                SprakType destination = LeftParam;
                SprakType source = other.LeftParam;
                if (!resolver.IsAssignable(source, destination))
                    return false;
            }

            if (RightParam == null)
            {
                if (other.RightParam != null)
                    return false;
            }
            else
            {
                SprakType destination = RightParam;
                SprakType source = other.RightParam;
                if (!resolver.IsAssignable(source, destination))
                    return false;
            }

            return true;
        }

        #region Equals
        public override int GetHashCode()
        {
            return HashCode.Combine(LeftParam, RightParam);
        }

        public bool Equals([AllowNull] OperatorTypeSignature other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            return other.LeftParam == LeftParam && other.RightParam == RightParam;
        }

        public sealed override bool Equals(object obj)
        {
            if (obj is OperatorTypeSignature parameter)
                return Equals(parameter);

            return false;
        }

        public static bool operator ==(OperatorTypeSignature one, OperatorTypeSignature two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(OperatorTypeSignature one, OperatorTypeSignature two)
        {
            return !Equals(one, two);
        }
        #endregion
    }
}
