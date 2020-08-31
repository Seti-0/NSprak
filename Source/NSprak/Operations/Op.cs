using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

using NSprak.Execution;

namespace NSprak.Operations
{
    public abstract class Op
    {
        public abstract string Name { get; }

        public virtual string ShortName => Name;

        public virtual object RawParam => null;

        public virtual bool StepAfterwards => true;

        public abstract void Execute(ExecutionContext context);

        public override string ToString()
        {
            return $"{Name}({RawParam})";
        }
    }
}
