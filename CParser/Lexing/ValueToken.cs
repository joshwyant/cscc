using CParser.Translation;

namespace CParser.Lexing
{
    public class ValueToken<T> : Token
    {
        public T Value { get; }
        public ValueToken(Terminal kind, int line, int column, string filename, T value)
            : base(kind, line, column, filename)
        {
            Value = value;
        }

        public override Token Copy(int line, int column, string filename)
        {
            return new ValueToken<T>(Kind, line, column, filename, Value);
        }
    }
}