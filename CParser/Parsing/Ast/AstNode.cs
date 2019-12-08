namespace CParser.Parsing.Ast
{
    public abstract class AstNode
    {
        public AstNode(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; }
        public int Column { get; }
    }
}