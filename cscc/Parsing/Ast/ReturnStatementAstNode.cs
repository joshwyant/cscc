namespace cscc.Parsing.Ast
{
    class ReturnStatementAstNode : JumpStatementAstNode
    {
        public ExpressionAstNode? Expression { get; }
        public ReturnStatementAstNode(
            ExpressionAstNode? expression,
            int line, int column)
            : base(line, column)
        {
            Expression = expression;
        }
    }
}