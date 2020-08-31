using NSprak.Language;

namespace NSprak.Expressions.Structure
{
    public class VariableInfo
    {
        public SprakType DeclaredType;

        public int Offset;

        public VariableInfo(SprakType type, int offset)
        {
            DeclaredType = type;
            Offset = offset;
        }
    }
}
