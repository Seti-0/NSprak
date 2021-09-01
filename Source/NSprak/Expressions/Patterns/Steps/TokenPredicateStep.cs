using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Tokens;

namespace NSprak.Expressions.Patterns.Steps
{
    public class TokenPredicateStep : PatternStep
    {
        public Predicate<Token> Predicate { get; }

        public TokenPredicateStep(Predicate<Token> predicate, string name) : base(name)
        {
            Predicate = predicate;
        }

        public override bool IsMatch(PatternState state)
        {
            return state.Enumerator.HasCurrent && Predicate(state.Enumerator.Current);
        }
    }
}
