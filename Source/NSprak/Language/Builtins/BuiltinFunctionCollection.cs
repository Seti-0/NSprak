using System;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Language.Builtins
{
    public class BuiltinFunctionCollection
    {
        private Dictionary<string, Dictionary<FunctionTypeSignature, BuiltInFunction>> _functionLookup
            = new Dictionary<string, Dictionary<FunctionTypeSignature, BuiltInFunction>>();

        private Dictionary<string, Dictionary<OperatorTypeSignature, BuiltInFunction>> _operatorLookup
            = new Dictionary<string, Dictionary<OperatorTypeSignature, BuiltInFunction>>();

        private Dictionary<ConversionTypeSignature, BuiltInFunction> _conversionLookup
            = new Dictionary<ConversionTypeSignature, BuiltInFunction>();

        private bool TryGetValue<K, V>(Dictionary<string, Dictionary<K, V>> main, string name, K key, out V value)
        {
            Dictionary<K, V> sub;

            if (!main.TryGetValue(name, out sub))
            {
                value = default;
                return false;
            }

            return sub.TryGetValue(key, out value);
        }

        private FunctionLookupResult<V> TryFindMatch<K, V>(Dictionary<K, V> source, Func<K, int> selector)
        {
            FunctionLookupResult<V> result = new FunctionLookupResult<V>();

            if (!source.Any())
                return result;

            IEnumerable<(int score, V value)> options =
                source
                .Select(x => (selector(x.Key), x.Value))
                .OrderBy(x => x.Item1);

            int minScore = options.First().score;

            options = options
                .TakeWhile(x => x.score == minScore);

            result.Items = options.Select(x => x.value).ToList();
            return result;
        }

        private FunctionLookupResult<V> TryFindMatch<K, V>(Dictionary<string, Dictionary<K, V>> source, string name, Func<K, int> selector)
        {
            Dictionary<K, V> subSource;

            if (!source.TryGetValue(name, out subSource))
                return FunctionLookupResult<V>.Empty;

            return TryFindMatch(subSource, selector);
        }

        private void Add<K, V>(Dictionary<string, Dictionary<K, V>> main, string name, K key, V value)
        {
            Dictionary<K, V> sub;

            if (!main.TryGetValue(name, out sub))
            {
                sub = new Dictionary<K, V>();
                main.Add(name, sub);
            }

            sub.Add(key, value);
        }

        public bool TryGetFunction(string name, FunctionTypeSignature signature, out BuiltInFunction value)
        {
            return TryGetValue(_functionLookup, name, signature, out value);
        }

        public bool TryGetOperator(string name, OperatorTypeSignature signature, out BuiltInFunction value)
        {
            return TryGetValue(_operatorLookup, name, signature, out value);
        }

        public bool TryGetConversion(ConversionTypeSignature signature, out BuiltInFunction value)
        {
            return _conversionLookup.TryGetValue(signature, out value);
        }

        public FunctionLookupResult<BuiltInFunction> TryFindMatch(string name, FunctionTypeSignature signature)
        {
            return TryFindMatch(_functionLookup, name, OverloadComparator.GetKey(signature));
        }

        public FunctionLookupResult<BuiltInFunction> TryFindMatch(string name, OperatorTypeSignature signature)
        {
            return TryFindMatch(_operatorLookup, name, OverloadComparator.GetKey(signature));
        }

        public void AddFunction(BuiltInFunction function)
        {
            string name = function.Signature.Name;
            FunctionTypeSignature key = function.Signature.TypeSignature;
            Add(_functionLookup, name, key, function);
        }

        public void AddOperator(string opName, OperatorTypeSignature signature, BuiltInFunction function)
        {
            Add(_operatorLookup, opName, signature, function);
        }

        public void AddConversion(ConversionTypeSignature signature, BuiltInFunction function)
        {
            _conversionLookup.Add(signature, function);
        }
    }
}
