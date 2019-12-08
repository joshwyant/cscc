namespace cscc.Parsing.Ast
{
    public abstract class ExpressionAstNode : AstNode
    {
        public ExpressionAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}