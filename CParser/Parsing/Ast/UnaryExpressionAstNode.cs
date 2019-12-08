using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class UnaryExpressionAstNode : ExpressionAstNode
    {
        public Terminal Terminal { get; }
        public ExpressionAstNode Expression { get; }
        public UnaryExpressionAstNode(Terminal terminal, ExpressionAstNode e, int line, int column)
            : base(line, column)
        {
            Terminal = terminal;
            Expression = e;
        }
    }
}