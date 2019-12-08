namespace cscc.Parsing.Ast
{
    class CaseLabelAstNode : LabelAstNode
    {
        public ExpressionAstNode ConstantExpression { get; }
        public CaseLabelAstNode(
            ExpressionAstNode constantExpression,
            int line, int column)
            : base(line, column)
        {
            ConstantExpression = constantExpression;
        }
    }
}