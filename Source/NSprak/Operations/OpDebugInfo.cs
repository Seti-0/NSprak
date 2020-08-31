using NSprak.Expressions;
using NSprak.Operations.Types;
using NSprak.Tokens;

namespace NSprak.Operations
{
    public class OpDebugInfo
    {
        public Op Op { get; }

        public int Index { get; }

        public Expression SourceExpression { get; }

        public Token FocusToken { get; }

        public string Comment { get; }

        public bool Breakpoint { get; set; }

        public OpDebugInfo(Op op, int index, Expression source, 
            Token focus = null, string comment = null)
        {
            Op = op;
            Index = index;
            SourceExpression = source;
            FocusToken = focus;
            Comment = comment;
        }

        public override string ToString()
        {
            if (Op is Pass)
            {
                if (Comment == null)
                    return nameof(Pass);

                else
                    return "# " + Comment;
            }
            else
            {
                string result = Op.Name;

                if (Op.RawParam != null)
                    result += $"({Op.RawParam})";

                if (Comment != null)
                    result += "#" + Comment;

                return result;
            }
        }
    }
}
