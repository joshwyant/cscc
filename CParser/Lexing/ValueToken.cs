using CParser.Translation;

namespace CParser.Lexing
{
    public class ValueToken<T> : Token
    {
        public T Value { get; }
        public bool IsValid { get; internal set; }

        public ValueToken(Terminal kind, int line, int column, string filename, T value)
            : base(kind, line, column, filename)
        {
            Value = value;
            IsValid = true;
        }

        public override Token Copy(int line, int column, string filename)
        {
            return new ValueToken<T>(Kind, line, column, filename, Value);
        }

        public override string? ToString()
        {
            return Value?.ToString();
        }
    }
}