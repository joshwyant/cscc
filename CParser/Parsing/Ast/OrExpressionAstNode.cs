using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class OrExpressionAstNode : BinaryExpressionAstNode
    {
        public OrExpressionAstNode(ExpressionAstNode e1, 
            ExpressionAstNode e2, int line, int column)
            : base(e1, Pipe, e2, line, column)
        {
        }
    }
}