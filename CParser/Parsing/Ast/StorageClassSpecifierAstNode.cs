using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class StorageClassSpecifierAstNode : SpecifierAstNode
    {
        public StorageClassSpecifierAstNode(Terminal terminal, int line, int column)
            : base(terminal, line, column)
        {
        }
    }
}