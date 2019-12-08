namespace cscc.Parsing.Ast
{
    abstract class InitializerAstNode : ExpressionAstNode
    {
        public InitializerAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}