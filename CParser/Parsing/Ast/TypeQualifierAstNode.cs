using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class TypeQualifierAstNode : SpecifierAstNode
    {
        public TypeQualifierAstNode(Terminal terminal, int line, int column)
            : base(terminal, line, column)
        {
        }
    }
}