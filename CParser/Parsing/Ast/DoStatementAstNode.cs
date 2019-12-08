namespace CParser.Parsing.Ast
{
    public class DoStatementAstNode : StatementAstNode
    {
        public StatementAstNode Statement { get; }
        public ExpressionAstNode Expression { get; }
        public DoStatementAstNode(
            StatementAstNode statement,
            ExpressionAstNode expression,
            int line, int column)
            : base(line, column)
        {
            Statement = statement;
            Expression = expression;
        }
    }
}