using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class UnaryExpressionAstNode : ExpressionAstNode
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