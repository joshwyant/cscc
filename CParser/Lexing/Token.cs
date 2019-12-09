using CParser.Translation;

namespace CParser.Lexing
{
    public class Token
    {
        public Token(Terminal kind, int line, int column, string filename)
        {
            Kind = kind;
            Line = line;
            Column = column;
            FileName = filename;
        }

        public Terminal Kind { get; }
        public int Line { get; }
        public int Column { get; }
        public string FileName { get; }

        public virtual Token Copy(int line, int column, string filename)
        {
            return new Token(Kind, line, column, filename);
        }

        public override bool Equals(object? obj)
        {
            return obj is Token ? (obj as Token)!.Kind == Kind : false;
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode();
        }

        public override string? ToString()
        {
            return TerminalMap.Name(Kind);
        }
    }
}