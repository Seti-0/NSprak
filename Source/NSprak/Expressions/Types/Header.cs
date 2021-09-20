namespace NSprak.Expressions.Types
{
    public abstract class Header : Expression
    {
        public abstract string FriendlyBlockName { get; }

        public bool RequiresScopeHint
        {
            get
            {
                bool? hint = ParentBlockHint?.ScopeHint?
                    .VariableDeclarations?.Count > 0;

                return hint ?? true;
            }
        }
    }
}
