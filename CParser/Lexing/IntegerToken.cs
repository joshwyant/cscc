namespace CParser.Lexing
{
    public class IntegerToken : ValueToken<long>
    {
        public bool HasUnsignedSpecifier { get; }
        public bool HasLongSpecifier { get; }
        public IntegerToken(int line, int column, string filename, long value, bool hasUnsignedSpecifier, bool hasLongSpecifier)
            : base(line, column, filename, value)
        {
            HasUnsignedSpecifier = hasUnsignedSpecifier;
            HasLongSpecifier = hasLongSpecifier;
        }
    }
}