using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class PostfixExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode Expression { get; }
        public Terminal Terminal { get; }
        public PostfixExpressionAstNode(ExpressionAstNode e, Terminal t,
            int line, int column)
            : base(line, column)
        {
            Expression = e;
            Terminal = t;
        }
    }
}