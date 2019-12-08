using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class PostfixPointerAccessExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode LValue { get; }
        public string Name { get; }
        public PostfixPointerAccessExpressionAstNode(
            ExpressionAstNode lValue, 
            string name, int line, int column)
            : base(line, column)
        {
            LValue = lValue;
            Name = name;
        }
    }
}