using CParser.Lexing;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
{
    public class LogicalAndExpressionAstNode : BinaryExpressionAstNode
    {
        public LogicalAndExpressionAstNode(ExpressionAstNode e1,
            ExpressionAstNode e2, int line, int column)
            : base(e1, LogicalAnd, e2, line, column)
        {
        }
    }
}