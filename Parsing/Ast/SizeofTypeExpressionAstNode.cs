using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class SizeofTypeExpressionAstNode : ExpressionAstNode
    {
        public TypeNameAstNode TypeName { get; }
        public SizeofTypeExpressionAstNode(TypeNameAstNode t, int line, int column)
            : base(line, column)
        {
            TypeName = t;
        }
    }
}