namespace cscc.Parsing.Ast
{
    abstract class ConstantExpressionAstNode : ExpressionAstNode
    {
        public ConstantExpressionAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}