using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class ShiftExpressionAstNode : BinaryExpressionAstNode
    {
        public ShiftExpressionAstNode(ExpressionAstNode e1, Terminal terminal, 
            ExpressionAstNode e2, int line, int column)
            : base(e1, terminal, e2, line, column)
        {
        }
    }
}