namespace CParser.Parsing.Ast
{
    public abstract class JumpStatementAstNode : StatementAstNode
    {
        public JumpStatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}