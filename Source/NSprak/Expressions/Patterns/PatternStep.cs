using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSprak.Messaging;
using NSprak.Tokens;

namespace NSprak.Expressions.Patterns
{
    public abstract class PatternStep
    {
        public EndStep EndStep;

        public bool DebuggerBreak { get; set; }

        public string Name { get; }

        public bool RequireEnd { get; set; }

        public bool AllowEnd { get; set; }

        public bool AllowLoopback { get; set; }

        public bool StayInPlace { get; set; }

        public List<PatternStep> Options { get; } = new List<PatternStep>();

        public PatternStep(string name)
        {
            Name = name;
        }

        public abstract bool IsMatch(PatternState state);

        public virtual void Execute(PatternState state)
        {
            if (IsMatch(state))
            {
                state.AddToCollection(state.Enumerator.Current);
            }
            else
            {
                if (state.Enumerator.HasCurrent)
                {
                    Token token = state.Enumerator.Current;
                    state.RaiseError(token, Messages.UnexpectedToken, token);
                }
                else if (state.Enumerator.HasPrevious)
                {
                    Token token = state.Enumerator.Previous;
                    state.RaiseError(token, Messages.UnexpectedEndOfLine);
                }
                else state.RaiseError(Messages.UnexpectedEndOfLine);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
