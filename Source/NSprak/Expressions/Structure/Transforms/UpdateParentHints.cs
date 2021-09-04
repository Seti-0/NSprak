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
            ApplyParent(root, null, null);
        }

        private void ApplyParent(Expression expression, Expression parent, Block parentBlock)
        {
            expression.ParentBlockHint = parentBlock;
            expression.ParentHint = parent;

            parent = expression;
            if (expression is Block block)
                parentBlock = block;

            foreach (Expression subExpr in expression.GetSubExpressions())
                ApplyParent(subExpr, parent, parentBlock);
        }
    }
}
