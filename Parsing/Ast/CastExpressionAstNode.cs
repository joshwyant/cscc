using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class CastExpressionAstNode : ExpressionAstNode
    {
        public CastExpressionAstNode(TypeNameAstNode typeName, ExpressionAstNode e, int line, int column)
            : base(line, column)
        {
        }
    }
}