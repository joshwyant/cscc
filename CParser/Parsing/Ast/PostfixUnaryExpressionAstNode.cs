using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public abstract class PostfixUnaryExpressionAstNode : ExpressionAstNode
    {
        public PostfixUnaryExpressionAstNode(ExpressionAstNode e, int line, int column)
            : base(line, column)
        {
        }
    }
}