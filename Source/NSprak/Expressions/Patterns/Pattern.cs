using NSprak.Messaging;
using NSprak.Tokens;

using System;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Expressions.Patterns
{
    public delegate object PatternEnd(MatchIterator iterator);

    public class Pattern : PatternElement
    {
        public string Name { get; }

        public PatternElement Value { get; set; }

        public Pattern(string name)
        {
            Name = name;
        }

        public bool TryMatch(IEnumerable<Token> tokens, 
            Messenger messenger, out Expression expression)
        {
            PatternState state = new PatternState(tokens, messenger);
            expression = null;

            // The enumerator starts at index -1, before the 
            // collection. The pattern expects a start at index 0.
            state.Enumerator.MoveNext();

            bool result = false;
            List<object> resultItems = null;

            if (OnCanExecute(state))
            {
                state.BeginCollection();

                result = OnExecute(state);

                resultItems = state.EndCollection();
            }
 

            if (state.Enumerator.HasCurrent)
            {
                state.Messenger.AtToken(
                    state.Enumerator.Current, Messages.UnexpectedToken);
            }

            if (!result)
            {
                if (state.Enumerator.AtEnd)
                {
                    if (state.Enumerator.HasPrevious)
                        state.Messenger.AtToken(
                            state.Enumerator.Previous, 
                            Messages.UnexpectedEndOfLine);
                }

                return false;
            }

            if (resultItems == null || resultItems.Count != 1)
                throw new Exception("Unexpected result from pattern match");

            if (resultItems[0] == null || resultItems[0] is Expression)
            {
                expression = (Expression)resultItems[0];
                return true;
            }
            else throw new Exception("Unexpected result from pattern match");
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            if (Value == null)
                throw new Exception("Pattern has no value");

            Value.Validate(context);
        }

        protected override bool OnCanExecute(PatternState state)
        {
            PatternElement value = Value;
            return value.CanExecute(state);
        }

        protected override bool OnExecute(PatternState state)
        {
            state.BeginCollection();

            PatternElement value = Value;
            bool success = value.Execute(state);

            List<object> items = state.EndCollection();

            if (!success)
                return false;

            if (state.Command != PatternCommand.End)
                throw new Exception(
                    "Successful pattern match with unexpected command");

            MatchIterator iterator = new MatchIterator(items);
            object result = state.EndDestination(iterator);
            state.AddToCollection(result);
            state.ClearCommand();

            return true;
        }
    }
}
