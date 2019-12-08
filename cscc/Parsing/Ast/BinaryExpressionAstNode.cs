using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class BinaryExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode Expression1 { get; }
        public Terminal Terminal { get; }
        public ExpressionAstNode Expression2 { get; }
        public BinaryExpressionAstNode(ExpressionAstNode e1,
            Terminal terminal, ExpressionAstNode e2, int line, int column)
            : base(line, column)
        {
            Expression1 = e1;
            Terminal = terminal;
            Expression2 = e2;
        }
    }
}