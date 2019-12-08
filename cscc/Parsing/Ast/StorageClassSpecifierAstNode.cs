using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class StorageClassSpecifierAstNode : SpecifierAstNode
    {
        public StorageClassSpecifierAstNode(Terminal terminal, int line, int column)
            : base(terminal, line, column)
        {
        }
    }
}