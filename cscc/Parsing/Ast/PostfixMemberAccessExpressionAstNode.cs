using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class PostfixMemberAccessExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode LValue { get; }
        public string Name { get; }
        public PostfixMemberAccessExpressionAstNode(
            ExpressionAstNode lValue, 
            string name, int line, int column)
            : base(line, column)
        {
            LValue = lValue;
            Name = name;
        }
    }
}