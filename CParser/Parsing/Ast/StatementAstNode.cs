namespace CParser.Parsing.Ast
{
    public abstract class StatementAstNode : AstNode
    {
        public StatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}