namespace CParser.Lexing
{
    public class FloatingToken : ValueToken<double>
    {
        public bool HasFloatSpecifier { get; }
        public FloatingToken(int line, int column, string filename, double value, bool hasFloatSpecifier)
            : base(Terminal.FloatingConstant, line, column, filename, value)
        {
            HasFloatSpecifier = hasFloatSpecifier;
        }
    }
}