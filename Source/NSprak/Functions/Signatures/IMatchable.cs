namespace NSprak.Functions.Signatures
{
    public interface IMatchable<TSelf> where TSelf : IMatchable<TSelf>
    {
        public bool Matches(TSelf other);
    }
}
