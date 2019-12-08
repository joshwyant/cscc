using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    abstract class PostfixUnaryExpressionAstNode : ExpressionAstNode
    {
        public PostfixUnaryExpressionAstNode(ExpressionAstNode e, int line, int column)
            : base(line, column)
        {
        }
    }
}