using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class SizeofTypeExpressionAstNode : ExpressionAstNode
    {
        public TypeNameAstNode TypeName { get; }
        public SizeofTypeExpressionAstNode(TypeNameAstNode t, int line, int column)
            : base(line, column)
        {
            TypeName = t;
        }
    }
}