using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class RelationalExpressionAstNode : BinaryExpressionAstNode
    {
        public RelationalExpressionAstNode(ExpressionAstNode e1, Terminal terminal, 
            ExpressionAstNode e2, int line, int column)
            : base(e1, terminal, e2, line, column)
        {
        }
    }
}