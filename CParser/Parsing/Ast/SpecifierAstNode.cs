using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public abstract class SpecifierAstNode : AstNode
    {
        public Terminal Terminal { get; }
        public SpecifierAstNode(Terminal terminal, int line, int column)
            : base(line, column)
        {
            Terminal = terminal;
        }
    }
}