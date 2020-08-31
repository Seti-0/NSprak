using NSprak.Expressions.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Structure
{
    public interface ITreeTransform
    {
        public void Apply(Block root, CompilationEnvironment environment);
    }
}
