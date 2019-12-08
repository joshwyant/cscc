namespace cscc.Parsing.Ast
{
    public class SwitchStatementAstNode : StatementAstNode
    {
        public ExpressionAstNode Expression { get; }
        public StatementAstNode Statement { get; }
        public SwitchStatementAstNode(
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