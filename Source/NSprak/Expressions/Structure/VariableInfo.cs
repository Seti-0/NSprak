using NSprak.Expressions.Types;
using NSprak.Language;

namespace NSprak.Expressions.Structure
{
    public class VariableInfo
    {
        public SprakType DeclaredType { get; }

        public int DeclarationEnd { get; }

        public Block Source { get; }

        public VariableInfo(SprakType type, int offset, Block source)
        {
            DeclaredType = type;
            DeclarationEnd = offset;
            Source = source;
        }
    }
}
