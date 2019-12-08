namespace cscc.Parsing.Ast
{
    class IfStatementAstNode : StatementAstNode
    {
        public ExpressionAstNode Expression { get; }
        public StatementAstNode Statement { get; }
        public StatementAstNode? ElseStatement { get; }
        public IfStatementAstNode(
            ExpressionAstNode expression,
            StatementAstNode statement,
            StatementAstNode? elseStatement,
            int line, int column)
            : base(line, column)
        {
            Expression = expression;
            Statement = statement;
            ElseStatement = elseStatement;
        }
    }
}