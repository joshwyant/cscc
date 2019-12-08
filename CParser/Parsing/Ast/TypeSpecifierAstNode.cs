using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class TypeSpecifierAstNode : SpecifierAstNode
    {
        public TypeSpecifierAstNode(Terminal terminal, int line, int column)
            : base(terminal, line, column)
        {
        }
    }
}