using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak.Expressions.Patterns.Steps
{
    public class DummyStep : PatternStep
    {
        public DummyStep() : base("Dummy")
        {
            StayInPlace = true;
        }

        public override bool IsMatch(PatternState state)
        {
            return true;
        }

        public override void Execute(PatternState state) { }
    }
}
