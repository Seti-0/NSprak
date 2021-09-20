using Microsoft.VisualBasic;
using NSprak.Functions.Resolution;
using NSprak.Language;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace NSprak.Functions.Signatures
{
    public enum InputSides
    {
        Right,
        Left,
        Both
    }


    public class OperatorTypeSignature : IEquatable<OperatorTypeSignature>
    {
        public InputSides Inputs { get; }

        public SprakType LeftParam { get; }

        public SprakType RightParam { get; }

        public string UniqueID
        {
            get
            {
                string input = Inputs switch
                {
                    InputSides.Both => LeftParam.UniqueID + "," + RightParam.UniqueID,
                    InputSides.Left => LeftParam.UniqueID,
                    InputSides.Right => RightParam.UniqueID,
                    _ => throw new Exception("Unrecognized input side")
                };

                return input;
            }
        }

        public OperatorTypeSignature(
            SprakType left, SprakType right, InputSides inputs)
        {
            LeftParam = left;
            RightParam = right;
            Inputs = inputs;
        }

        public override string ToString()
        {
            return UniqueID;
        }

        public bool CanAccept(OperatorTypeSignature other, AssignmentResolver resolver)
        {
            if (other.Inputs != Inputs)
                return false;

            if (Inputs == InputSides.Both || Inputs == InputSides.Left)
            {
                SprakType destination = LeftParam;
                SprakType source = other.LeftParam;
                if (!resolver.IsAssignable(source, destination))
                    return false;
            }

            if (Inputs == InputSides.Both || Inputs == InputSides.Right)
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
            return HashCode.Combine(LeftParam, RightParam, Inputs);
        }

        public bool Equals([AllowNull] OperatorTypeSignature other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            return other.LeftParam == LeftParam 
                && other.RightParam == RightParam
                && other.Inputs == Inputs;
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
