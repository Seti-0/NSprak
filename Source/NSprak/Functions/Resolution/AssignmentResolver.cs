using NSprak.Functions.Signatures;
using NSprak.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSprak.Functions.Resolution
{
    public class AssignmentResolver
    {
        private readonly Dictionary<SprakType, HashSet<SprakType>> _possibleConversions
            = new Dictionary<SprakType, HashSet<SprakType>>();

        private readonly Dictionary<ConversionTypeSignature, bool> _informationLoss
            = new Dictionary<ConversionTypeSignature, bool>();

        public AssignmentResolver(IEnumerable<Library> libraries)
        {
            IEnumerable<ConversionTypeSignature> signatures = libraries
                .SelectMany(x => x.Conversions.Keys);

            foreach (ConversionTypeSignature signature in signatures)
                AddConversion(signature);
        }

        public bool IsAssignable(SprakType source, SprakType destination)
        {
            if (IsDirect(source, destination))
                return true;

            HashSet<SprakType> possibleSources;

            if (!_possibleConversions.TryGetValue(destination, out possibleSources))
                return false;

            return possibleSources.Contains(destination);
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

            // At somepoint, the assignment resolver should be modified so that
            // a source of any yields "IsDirect" as true, but "IsPerfect" as
            // false, perhaps? Or maybe this is a special case better handled
            // by the code using the resolution.

            if (destination == SprakType.Any || source == SprakType.Any)
                return true;

            if (destination == source)
                return true;

            if (destination == SprakType.String)
                return true;

            return false;
        }
    }
}
