namespace CParser.Parsing.Ast
{
    public class InitializerExpressionAstNode : InitializerAstNode
    {
        public ExpressionAstNode Expression { get; }
        public InitializerExpressionAstNode(
            ExpressionAstNode e, int line, int column)
            : base(line, column)
        {
            Expression = e;
        }
    }
}