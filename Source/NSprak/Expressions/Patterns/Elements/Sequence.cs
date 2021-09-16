using System;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Expressions.Patterns.Elements
{
    public class Sequence : PatternElement
    {
        public List<PatternElement> Elements { get; }

        public Sequence(params PatternElement[] items)
        {
            Elements = items.ToList();
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            if (Elements.Count == 0)
                throw new Exception("Sequence has no elements");

            context.BeginSequenceScope();

            foreach (PatternElement element in Elements)
                element.Validate(context);

            if (Elements.Last() is OptionalElement option
                && option.Value is EndElement)
                throw new Exception("A final optional end will never execute");

            context.EndSequenceScope();
        }

        protected override bool OnCanExecute(PatternState state)
        {
            foreach (PatternElement element in Elements)
            {
                // The special case for optional elements isn't really needed here.
                if (element is OptionalElement option)
                {
                    if (option.Value.CanExecute(state))
                        return true;
                }
                else 
                    return element.CanExecute(state);
            }

            return false;
        }

        protected override bool OnExecute(PatternState state)
        {
            int lastEnumeratorIndex = state.Enumerator.Index;

            EndElement optionalEnd = null;

            for (int i = 0; i < Elements.Count; i++)
            {
                PatternElement element = Elements[i];

                // Special case for optional end elements
                // They can always be executed, but whether or not they 
                // should be depends on if the next element can be.
                if (element is OptionalElement option 
                    && option.Value is EndElement endElement)
                {
                    optionalEnd = endElement;
                    continue;
                }

                if (!element.CanExecute(state))
                {
                    if (optionalEnd != null)
                    {
                        optionalEnd.Execute(state);
                        return true;
                    }

                    return false;
                }

                bool success = element.Execute(state);

                if (!success)
                    return false;

                switch (state.Command)
                {
                    case PatternCommand.Break:
                        state.ClearCommand();
                        return true;

                    case PatternCommand.Loopback:
                        state.ClearCommand();

                        if (state.Enumerator.Index == lastEnumeratorIndex)
                            throw new Exception("Possible infinite loopback detected");

                        // i will be incremented to 0 at the end of the iteration
                        // and the loop will begin again. I'm curious, though,
                        // is there a more readable way to designate that the loop
                        // should start over?
                        i = -1;
                        continue;

                    case PatternCommand.End:
                        return true;
                }

                // An optional end only applied to the CanExecute
                // until the first non-optional, non-command, element after it.
                if (!(element is OptionalElement))
                    optionalEnd = null;
            }

            return true;
        }
    }
}
