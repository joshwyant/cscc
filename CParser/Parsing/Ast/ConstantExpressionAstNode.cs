namespace CParser.Parsing.Ast
{
    public abstract class ConstantExpressionAstNode : ExpressionAstNode
    {
        public ConstantExpressionAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}