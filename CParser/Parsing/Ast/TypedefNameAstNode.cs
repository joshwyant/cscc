using CParser.Lexing;
using CParser.Translation;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
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