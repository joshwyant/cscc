using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class CastExpressionAstNode : ExpressionAstNode
    {
        public CastExpressionAstNode(TypeNameAstNode typeName, ExpressionAstNode e, int line, int column)
            : base(line, column)
        {
        }
    }
}