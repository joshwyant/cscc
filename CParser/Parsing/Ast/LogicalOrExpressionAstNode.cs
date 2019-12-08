using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class LogicalOrExpressionAstNode : BinaryExpressionAstNode
    {
        public LogicalOrExpressionAstNode(ExpressionAstNode e1,
            ExpressionAstNode e2, int line, int column)
            : base(e1, LogicalOr, e2, line, column)
        {
        }
    }
}