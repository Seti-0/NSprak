using System;
using System.Collections.Generic;
using System.Linq;

namespace NSprak.Expressions.Patterns.Elements
{
    public class Options : PatternElement
    {
        public List<PatternElement> Elements { get; }

        public Options(params PatternElement[] items)
        {
            Elements = items.ToList();
        }

        protected override void OnValidate(PatternCheckContext context)
        {
            if (Elements.Count == 0)
                throw new Exception("Option has no elements!");

            foreach (PatternElement element in Elements)
            {
                element.Validate(context);

                if (element.Optional)
                    throw new Exception("Optional Option element");
            }
        }

        protected override bool OnCanExecute(PatternState state)
        {
            foreach (PatternElement element in Elements)
                if (element.CanExecute(state))
                    return true;

            return false;
        }

        protected override bool OnExecute(PatternState state)
        {
            foreach (PatternElement element in Elements)
                if (element.CanExecute(state))
                    return element.Execute(state);

            return false;
        }
    }
}
