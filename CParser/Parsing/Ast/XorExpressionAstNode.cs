using CParser.Lexing;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
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