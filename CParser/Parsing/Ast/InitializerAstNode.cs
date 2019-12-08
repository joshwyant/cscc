namespace CParser.Parsing.Ast
{
    public abstract class InitializerAstNode : ExpressionAstNode
    {
        public InitializerAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}