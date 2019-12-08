using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class AndExpressionAstNode : BinaryExpressionAstNode
    {
        public AndExpressionAstNode(ExpressionAstNode e1, 
            ExpressionAstNode e2, int line, int column)
            : base(e1, Ampersand, e2, line, column)
        {
        }
    }
}