using System;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Functions.Resolution
{
    public static class TwoStepLookup
    {
        public static TwoStepLookup<TKey, TValue> ToLookup<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, string> nameSelector,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector)
        {
            List<string> names = source.Select(nameSelector).ToList();
            List<TKey> keys = source.Select(keySelector).ToList();
            List<TValue> values = source.Select(valueSelector).ToList();

            return new TwoStepLookup<TKey, TValue>(names, keys, values);
        }

        public static TwoStepLookup<TKey, TValue> ToLookup<TKey, TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, string> nameSelector,
            Func<TValue, TKey> keySelector)
        {
            List<string> names = source.Select(nameSelector).ToList();
            List<TKey> keys = source.Select(keySelector).ToList();

            return new TwoStepLookup<TKey, TValue>(names, keys, source.ToList());
        }
    }

    public class TwoStepLookup<TKey, TValue>
    {
        private readonly Dictionary<string, Dictionary<TKey, TValue>> _lookup
            = new Dictionary<string, Dictionary<TKey, TValue>>();

        public TwoStepLookup(List<string> names, List<TKey> keys, List<TValue> values)
        {
            for (int i = 0; i < values.Count; i++)
                Add(names[i], keys[i], values[i]);
        }

        private void Add(string name, TKey key, TValue value)
        {
            Dictionary<TKey, TValue> innerLookup;

            if (!_lookup.TryGetValue(name, out innerLookup))
            {
                innerLookup = new Dictionary<TKey, TValue>();
                _lookup.Add(name, innerLookup);
            }

            innerLookup.Add(key, value);
        }

        public IEnumerable<TValue> GetValues(string name)
        {
            Dictionary<TKey, TValue> innerLookup;

            if (_lookup.TryGetValue(name, out innerLookup))
                return innerLookup.Values;

            return Enumerable.Empty<TValue>();
        }

        public bool TryGetValue(string name, TKey key, out TValue value)
        {
            Dictionary<TKey, TValue> innerLookup;

            if (!_lookup.TryGetValue(name, out innerLookup))
            {
                value = default;
                return false;
            }

            return innerLookup.TryGetValue(key, out value);
        }

        public bool TryGetOptions(string name, out Dictionary<TKey, TValue> options)
        {
            return _lookup.TryGetValue(name, out options);
        }
    }
}
