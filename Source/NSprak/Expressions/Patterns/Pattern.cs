using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSprak.Messaging;
using NSprak.Tokens;

namespace NSprak.Expressions.Patterns
{
    public delegate object EndStep(MatchIterator iterator);

    public class PatternMatchResult
    {
        public object Item;
        public bool Error;
    }

    public class Pattern
    {
        public string Name { get; }

        public bool AllowEmpty { get; set; }

        public List<PatternStep> EntryPoints = new List<PatternStep>();

        public Pattern(string name)
        {
            Name = name;
        }

        public bool IsMatch(PatternState state)
        {
            return EntryPoints.Any(x => x.IsMatch(state));
        }

        public PatternMatchResult Apply(IEnumerable<Token> tokens, IMessenger messenger)
        {
            PatternState state = new PatternState(tokens, messenger);
            state.Enumerator.MoveNext();
            return ApplyWithin(state);
        }

        public PatternMatchResult ApplyWithin(PatternState state)
        {
            PatternEnumerator tokens = state.Enumerator;
            state.StartCollection();

            PatternStep currentStep = null;

            if (!tokens.HasCurrent)
            {
                if (!AllowEmpty)
                    state.RaiseError(Messages.UnexpectedEnd);
            }
            else
            {
                while (tokens.HasCurrent)
                {
                    if (TryGetNextStep(currentStep, state, out PatternStep nextStep))
                        currentStep = nextStep;

                    else break;

                    //if (currentStep.DebuggerBreak)
                        //Debugger.Break();

                    state.Steps.Add(currentStep);
                    currentStep.Execute(state);

                    if (currentStep.RequireEnd && tokens.HasNext)
                        state.RaiseError(tokens.Current, 
                            Messages.UnexpectedTokenAtEnd, tokens.Current);

                    if (state.Error) break;

                    if (!currentStep.StayInPlace)
                        tokens.MoveNext();
                }
            }

            MatchIterator collection = new MatchIterator(state.EndCollection());

            PatternMatchResult result = new PatternMatchResult();

            if (currentStep != null && !currentStep.AllowEnd)
                state.RaiseError(null, "Unexpected end of line");

            if (state.Error)
                result.Error = true;

            else if (currentStep != null)
            {
                // The end step is currently not allowed to raise an error message, though it 
                // may throw an exception if an internal error occurs
                result.Item = currentStep.EndStep(collection);
            }

            return result;
        }

        private bool TryGetNextStep(PatternStep current, PatternState state, out PatternStep nextStep)
        {
            IEnumerable<PatternStep> options;

            if (current == null)
                options = EntryPoints.Where(x => x.IsMatch(state));

            else options = current.Options.Where(x => x.IsMatch(state));

            int n = options.Count();

            if (n > 1)
                throw new NotSupportedException("Ambiguous statement juncture");

            else if (n == 0)
            {
                if (current != null && current.AllowLoopback)
                    return TryGetNextStep(null, state, out nextStep);

                if (current == null ||!current.AllowEnd)
                {
                    Token token = state.Enumerator.Current;
                    state.RaiseError(token, Messages.UnexpectedToken, token);
                }

                nextStep = null;
                return false;
            }

            nextStep = options.First();
            return true;
        }

        public override string ToString()
        {
            return "Pattern: "+Name;
        }
    }
}
