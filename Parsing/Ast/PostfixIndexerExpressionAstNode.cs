using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class PostfixIndexerExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode LValue { get; }
        public ExpressionAstNode Index { get; }
        public PostfixIndexerExpressionAstNode(
            ExpressionAstNode lValue, 
            ExpressionAstNode index, int line, int column)
            : base(line, column)
        {
            LValue = lValue;
            Index = index;
        }
    }
}