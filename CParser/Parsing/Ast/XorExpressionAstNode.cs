using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class XorExpressionAstNode : BinaryExpressionAstNode
    {
        public XorExpressionAstNode(ExpressionAstNode e1, 
            ExpressionAstNode e2, int line, int column)
            : base(e1, Caret, e2, line, column)
        {
        }
    }
}