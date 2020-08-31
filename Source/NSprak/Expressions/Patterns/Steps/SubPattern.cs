using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Expressions.Patterns.Steps
{
    public class SubPatternStep : PatternStep
    {
        public Pattern Target { get; }

        public SubPatternStep(Pattern target) : base("SubPattern: "+target.Name)
        {
            StayInPlace = true;
            Target = target;
        }

        public override bool IsMatch(PatternState state)
        {
            return Target.EntryPoints.Any(x => x.IsMatch(state));
        }

        public override void Execute(PatternState state)
        {
            PatternMatchResult result = Target.ApplyWithin(state);

            if (result.Error)
            {
                // TODO: we should be able to keep track of the token here somehow
                state.RaiseError(null, "Failed to apply subpattern");
            }
            else
            {
                state.AddToCollection(result.Item);
            }
        }
    }
}
