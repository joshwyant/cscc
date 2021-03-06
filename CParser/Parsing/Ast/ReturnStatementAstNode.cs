namespace CParser.Parsing.Ast
{
    public class ReturnStatementAstNode : JumpStatementAstNode
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