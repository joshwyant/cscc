using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class TypedefNameAstNode : SpecifierAstNode
    {
        public Symbol Symbol { get; }
        public TypedefNameAstNode(Symbol symbol, int line, int column)
            : base(TypedefName, line, column)
        {
            Symbol = symbol;
        }
    }
}