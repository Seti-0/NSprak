using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace NSprak.Functions.Signatures
{
    public class FunctionSignature
    {
        public string Namespace { get; }

        public string Name { get; }

        public string FullName => $"{Namespace}.{Name}";

        public FunctionTypeSignature TypeSignature { get; }

        public string UniqueID
        {
            get => $"{Namespace}.{Name}({TypeSignature.UniqueID})";
        }

        public FunctionSignature(string libraryID, string name, FunctionTypeSignature typeSignature)
        {
            Namespace = libraryID;
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
            // This is very particular. 

            // The name of a function is "almost" its unique key, in that
            // most functions will only have one overload, and it is likely
            // that none will have very many.

            // So the name alone seems a fine (and cheap) hashcode, even as
            // equality requires checking the full type signature.

            return Name.GetHashCode();
        }

        public bool Equals([AllowNull] FunctionSignature other)
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

        public static bool operator ==(FunctionSignature one, FunctionSignature two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(FunctionSignature one, FunctionSignature two)
        {
            return !Equals(one, two);
        }
        #endregion
    }
}
