namespace cscc.Parsing.Ast
{
    abstract class JumpStatementAstNode : StatementAstNode
    {
        public JumpStatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}