using NSprak.Language.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSprak.Language
{
    public class AssignmentResolver
    {
        private Dictionary<SprakType, HashSet<SprakType>> _possibleConversions = new Dictionary<SprakType, HashSet<SprakType>>();
        private Dictionary<ConversionTypeSignature, bool> _informationLoss = new Dictionary<ConversionTypeSignature, bool>();

        public AssignmentResolver(IEnumerable<Library> libraries)
        {
            foreach (ConversionTypeSignature signature in libraries.SelectMany(x => x.Conversions.Keys))
                AddConversion(signature);
        }

        public AssignmentResolver(IEnumerable<ConversionTypeSignature> conversions)
        {
            foreach (ConversionTypeSignature signature in conversions)
                AddConversion(signature);
        }

        private void AddConversion(ConversionTypeSignature conversion)
        {
            HashSet<SprakType> innerSet;

            if (!_possibleConversions.TryGetValue(conversion.Output, out innerSet))
            {
                innerSet = new HashSet<SprakType>();
                _possibleConversions.Add(conversion.Output, innerSet);
            }

            innerSet.Add(conversion.Input);

            if (conversion.IsPerfectHint.HasValue)
                _informationLoss.Add(conversion, conversion.IsPerfectHint.Value);
        }

        private bool IsDirect(SprakType source, SprakType destination)
        {
            // Obviously if the destination accepts "any" then the current value is okay
            // The source being "any" is a problem - there is no way of checking.
            // Current the principle source of "any" is the array indexer, since arrays can contain any combination of values.
            if (destination == SprakType.Any || source == SprakType.Any)
                return true;

            if (destination == source)
                return true;

            if (destination == SprakType.String)
                return true;

            return false;
        }

        public bool IsAssignable(SprakType source, SprakType destination)
        {
            if (IsDirect(source, destination))
                return true;

            HashSet<SprakType> innerSet;

            if (!_possibleConversions.TryGetValue(destination, out innerSet))
                return false;

            return innerSet.Contains(destination);
        }

        public bool IsPerfect(SprakType source, SprakType destination)
        {
            if (IsDirect(source, destination))
                return true;

            ConversionTypeSignature signature = new ConversionTypeSignature(source, destination);

            if (_informationLoss.TryGetValue(signature, out bool perfect))
                return perfect;

            else throw new ArgumentException($"Unrecognized conversion from {source} to {destination}");
        }
    }
}
