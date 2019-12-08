using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    abstract class SpecifierAstNode : AstNode
    {
        public Terminal Terminal { get; }
        public SpecifierAstNode(Terminal terminal, int line, int column)
            : base(line, column)
        {
            Terminal = terminal;
        }
    }
}