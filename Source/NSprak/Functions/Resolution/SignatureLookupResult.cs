using System;
using NSprak.Operations;

namespace NSprak.Functions.Resolution
{
    public class SignatureLookupResult
    {
        public bool Success;
        public bool Ambiguous;

        public FunctionInfo FunctionInfo;

        public Func<Op> OpBuilder;
        public BuiltInFunction BuiltInFunction;
    }
}
