using System;
using System.Text;

using NSprak.Expressions.Patterns.Elements;

namespace NSprak.Expressions.Patterns
{
    public class PatternCheckContext
    {
        private int _sequenceScope = 0;

        public bool InSequenceScope => _sequenceScope > 0;

        public void BeginSequenceScope()
        {
            _sequenceScope++;
        }

        public void EndSequenceScope()
        {
            if (_sequenceScope == 0)
                throw new Exception("No sequence scope to end!");

            _sequenceScope--;
        }
    }

    public abstract class PatternElement
    {
        public static PatternElement operator &(PatternElement a, PatternElement b)
        {
            if (a is Sequence aSeq)
            {
                aSeq.Elements.Add(b);
                return a;
            }
            else if (b is Sequence bSeq)
            {
                bSeq.Elements.Insert(0, a);
                return b;
            }

            return new Sequence(a, b);
        }

        public static PatternElement operator |(PatternElement a, PatternElement b)
        {
            if (a is Options aOpt)
            {
                aOpt.Elements.Add(b);
                return a;
            }
            else if (b is Options bOpt)
            {
                bOpt.Elements.Insert(0, a);
                return b;
            }

            return new Options(a, b);
        }

        // In theory, the pattern elements have no state beyond their
        // initial configuration. (The initial config can be considered 
        // frozen, though this is not enforced)
        private bool _checked = false;

        // Rather than try and recursively create a sensible ToString on the
        // fly, the descriptions are generated externally by a class that 
        // keeps track of cycles and loops in case something goes wrgon.
        private string _sourceText = "No source string specified";

        public void Validate()
        {
            Validate(new PatternCheckContext());
        }

        public void Validate(PatternCheckContext context)
        {
            if (_checked)
                return;

            // If checking a child causes this element to be 
            // checked again, don't get caught in a loop.
            _checked = true;

            OnValidate(context);
        }

        public bool CanExecute(PatternState state)
        {
            TraceItem trace = state.Trace.OnCanExecute(this);

            bool result = OnCanExecute(state);

            trace.Update(result);
            return result;
        }


        public bool Execute(PatternState state)
        {
            TraceItem trace = state.Trace.OnExecute(this);

            bool result = OnExecute(state);

            trace.Update(result);
            return result;
        }

        protected abstract bool OnCanExecute(PatternState state);

        protected abstract bool OnExecute(PatternState state);

        protected abstract void OnValidate(PatternCheckContext context);

        public void SpecifySourceText(string sourceText)
        {
            _sourceText = sourceText;
        }

        public override string ToString()
        {
            if (_sourceText == null)
                return "(Source text not specified)";

            return _sourceText;
        }
    }
}
