namespace cscc.Parsing.Ast
{
    class WhileStatementAstNode : StatementAstNode
    {
        public ExpressionAstNode Expression { get; }
        public StatementAstNode Statement { get; }
        public WhileStatementAstNode(
            ExpressionAstNode expression,
            StatementAstNode statement,
            int line, int column)
            : base(line, column)
        {
            Expression = expression;
            Statement = statement;
        }
    }
}