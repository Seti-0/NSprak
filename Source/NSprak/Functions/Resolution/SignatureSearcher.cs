using System;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Functions.Resolution
{
    public static class SignatureSearcher
    {
        public class Checks<TSignature>
        {
            public Predicate<TSignature> Matches;
            public Func<TSignature, int> ExactComparison;
            public Func<TSignature, int> LosslessComparison;
        }

        public class Result<TKey, TValue>
        {
            public TKey Key;
            public TValue Value;
            public bool Success;
            public bool Ambiguous;
        }

        private class Option<TKey, TValue>
        {
            public TKey Key;
            public TValue Value;
            public int ExactRating;
            public int LossRating;
        }

        public static Result<TKey, TValue> Search<TSignature, TKey, TValue>(
            Dictionary<TKey, TValue> source, 
            Checks<TSignature> checks, 
            Func<TKey, TSignature> signatureSelector)
        {
            Result<TKey, TValue> result = new Result<TKey, TValue>();

            Option<TKey, TValue> CreateOption(KeyValuePair<TKey, TValue> item)
            {
                TSignature signature = signatureSelector(item.Key);

                return new Option<TKey, TValue>
                {
                    Key = item.Key,
                    Value = item.Value,
                    ExactRating = checks.ExactComparison(signature),
                    LossRating = checks.LosslessComparison(signature)
                };
            }

            IEnumerable<Option<TKey, TValue>> options;

            options = source
                .Where(x => checks.Matches(signatureSelector(x.Key)))
                .Select(CreateOption)
                .ToList();

            if (!options.Any())
                return result;

            if (options.Count() > 1)
            {
                int minKey = options
                    .Select(x => x.ExactRating)
                    .Min();

                options = options
                    .Where(x => x.ExactRating == minKey);

                if (options.Count() > 1)
                {
                    // This is a bug I really don't understand. It seems that
                    // the select & min below clears the options enumeration,
                    // though it should not. This copy is therefore needed.
                    var options2 = options.ToList();

                    minKey = options
                        .Select(x => x.LossRating)
                        .Min();

                    options = options2
                        .Where(x => x.LossRating == minKey);

                    if (options.Count() > 1)
                        result.Ambiguous = true;
                }
            }

            Option<TKey, TValue> choice = options.First();

            result.Success = true;
            result.Key = choice.Key;
            result.Value = choice.Value;
            return result;
        }
    }
}
