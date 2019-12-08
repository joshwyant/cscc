using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    class LogicalAndExpressionAstNode : BinaryExpressionAstNode
    {
        public LogicalAndExpressionAstNode(ExpressionAstNode e1,
            ExpressionAstNode e2, int line, int column)
            : base(e1, LogicalAnd, e2, line, column)
        {
        }
    }
}