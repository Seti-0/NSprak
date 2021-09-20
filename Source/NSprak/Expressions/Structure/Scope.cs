using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Structure
{
    public class Scope
    {
        public IDictionary<string, VariableInfo> VariableDeclarations { get; }

        public Scope()
        {
            VariableDeclarations = new Dictionary<string, VariableInfo>();
        }
    }
}
