using NSprak.Expressions.Types;
using NSprak.Functions.Signatures;
using NSprak.Language;
using System.Collections.Generic;

namespace NSprak.Functions
{
    public class FunctionInfo
    {
        public FunctionSignature Signature { get; }

        public SprakType ReturnType { get; }

        public string Name => Signature.Name;

        public string Namespace => Signature.Namespace;

        public IReadOnlyList<SprakType> Parameters => Signature.TypeSignature.Parameters;

        public FunctionInfo(FunctionSignature signature, SprakType returnType)
        {
            Signature = signature;
            ReturnType = returnType;
        }

        public FunctionInfo(FunctionHeader header)
        {
            Signature = header.Signature;
            ReturnType = header.ReturnType;
        }

        public override string ToString()
        {
            return $"{ReturnType} {Signature.UniqueID}";
        }
    }
}
