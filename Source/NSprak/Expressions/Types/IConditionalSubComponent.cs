using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Types
{
    public interface IConditionalSubComponent
    {
        public string EndLabelHint { get; set; }

        public IConditionalSubComponent NextConditionalComponentHint { get; }
    }
}
