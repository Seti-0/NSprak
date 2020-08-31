using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NSprak.Language.Builtins
{
    public class OperatorSignature
    {
        public string Name { get; }

        public string FullName => $"{Name}";

        public OperatorTypeSignature TypeSignature { get; }

        public string UniqueID
        {
            get => $"{Name}({TypeSignature.UniqueID})";
        }

        public OperatorSignature(string name, OperatorTypeSignature typeSignature)
        {
            Name = name;
            TypeSignature = typeSignature;
        }

        public override string ToString()
        {
            return UniqueID;
        }

        #region Equals

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, TypeSignature);
        }

        public bool Equals([AllowNull] OperatorSignature other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            if (other.Name != Name)
                return false;

            if (other.TypeSignature != TypeSignature)
                return false;

            return true;
        }

        public sealed override bool Equals(object obj)
        {
            if (obj is FunctionSignature parameter)
                return Equals(parameter);

            return false;
        }

        public static bool operator ==(OperatorSignature one, OperatorSignature two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(OperatorSignature one, OperatorSignature two)
        {
            return !Equals(one, two);
        }
        #endregion
    }
}
