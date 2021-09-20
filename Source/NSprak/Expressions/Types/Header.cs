namespace NSprak.Expressions.Types
{
    public abstract class Header : Expression
    {
        public abstract string FriendlyBlockName { get; }

        public bool CombinedScopeHint { get; set; }

        public bool RequiresScopeHint
        {
            get
            {
                bool? hint = ParentBlockHint?.VariableDeclarationsHint?.Count > 0;

                bool result = hint ?? true;
                result |= CombinedScopeHint;

                return result;
            }
        }
    }
}
