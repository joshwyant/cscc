namespace cscc.Parsing.Ast
{
    class BreakStatementAstNode : JumpStatementAstNode
    {
        public BreakStatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}