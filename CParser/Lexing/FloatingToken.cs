namespace CParser.Lexing
{
    public class FloatingToken : ValueToken<decimal>
    {
        public bool HasFloatSpecifier { get; }
        public FloatingToken(int line, int column, string filename, decimal value, bool hasFloatSpecifier)
            : base(line, column, filename, value)
        {
            HasFloatSpecifier = hasFloatSpecifier;
        }
    }
}