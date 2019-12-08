namespace CParser.Lexing
{
    public class IntegerToken : ValueToken<ulong>
    {
        public bool HasUnsignedSpecifier { get; }
        public bool HasLongSpecifier { get; }
        public IntegerToken(int line, int column, string filename, ulong value, bool hasUnsignedSpecifier, bool hasLongSpecifier)
            : base(Terminal.IntegerConstant, line, column, filename, value)
        {
            HasUnsignedSpecifier = hasUnsignedSpecifier;
            HasLongSpecifier = hasLongSpecifier;
        }
    }
}