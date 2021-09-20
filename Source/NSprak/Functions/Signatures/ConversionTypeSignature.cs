using NSprak.Language;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NSprak.Functions.Signatures
{
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
}
