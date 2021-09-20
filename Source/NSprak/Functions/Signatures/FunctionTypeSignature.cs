using NSprak.Functions.Resolution;
using NSprak.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NSprak.Functions.Signatures
{
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
}
