namespace NSprak.Expressions.Types
{
    public abstract class Header : Expression
    {
        public abstract string FriendlyBlockName { get; }

        public bool RequiresScopeHint
        {
            get
            {
                bool? hint = ParentBlockHint?.VariableDeclarationsHint?.Count > 0;

                if (hint.HasValue)
                    return hint.Value;

                return true;
            }
        }
    }
}
