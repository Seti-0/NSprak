using NSprak.Expressions.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Expressions.Structure.Transforms
{
    public class UpdateParentHints : ITreeTransform
    {
        public void Apply(Block root, CompilationEnvironment environment)
        {
            ApplyParent(root, null);
        }

        private void ApplyParent(Expression expression, Block parent)
        {
            expression.ParentHint = parent;

            if (expression is Block block)
                parent = block;

            foreach (Expression subExpr in expression.GetSubExpressions())
                ApplyParent(subExpr, parent);
        }
    }
}
