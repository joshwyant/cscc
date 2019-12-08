using CParser.Lexing;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
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