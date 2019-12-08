using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class TypeQualifierAstNode : SpecifierAstNode
    {
        public TypeQualifierAstNode(Terminal terminal, int line, int column)
            : base(terminal, line, column)
        {
        }
    }
}