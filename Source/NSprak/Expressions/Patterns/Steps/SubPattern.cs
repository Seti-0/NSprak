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

            // There is no point in logging a message here, the error message
            // useful to the user will have been logged by the subpattern step.
            if (result.Error)
                // We do need to raise the error flag though, so that the parent
                // pattern knows to exit.
                state.RaiseError();

            else state.AddToCollection(result.Item);
        }
    }
}
