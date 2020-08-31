using NSprak.Execution;
using NSprak.Language.Builtins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace NSprak.Language
{
    public class SprakConverter
    {
        private SignatureResolver _resolver;
        private ExecutionContext _context;

        public SprakConverter(SignatureResolver resolver, ExecutionContext context)
        {
            _resolver = resolver;
            _context = context;
        }

        public bool TryConvertValue(Value source, SprakType destinationType, out Value destination)
        {
            if (source.Type == destinationType)
            {
                destination = source;
                return true;
            }    

            if (destinationType == SprakType.Any)
            {
                destination = source;
                return true;
            }

            if (destinationType == SprakType.String)
            {
                destination = source.ToSprakString();
                return true;
            }

            ConversionTypeSignature signature = new ConversionTypeSignature(source.Type, destinationType);

            SignatureLookupResult result = _resolver.TryFindMatch(signature);

            if (!result.Success)
            {
                destination = null;
                return false;
            }

            destination = result.BuiltInFunction.Call(new Value[] { source }, _context);
            return true;
        }
    }
}
