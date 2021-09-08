using NSprak.Language;

namespace NSprak.Expressions.Structure
{
    public class VariableInfo
    {
        public SprakType DeclaredType;

        public int DeclarationEnd;

        public VariableInfo(SprakType type, int offset)
        {
            DeclaredType = type;
            DeclarationEnd = offset;
        }
    }
}
