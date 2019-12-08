namespace cscc.Parsing.Ast
{
    abstract class ExpressionAstNode : AstNode
    {
        public ExpressionAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}