namespace NSprakIDE.Logging
{
    public interface ISimpleWriter
    {
        public void Write(string text);

        public void WriteLine(string text = "");
    }
}
