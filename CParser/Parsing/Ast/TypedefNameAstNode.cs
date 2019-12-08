using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class TypedefNameAstNode : SpecifierAstNode
    {
        public string Name { get; }
        public TypedefNameAstNode(string name, int line, int column)
            : base(TypedefName, line, column)
        {
            Name = name;
        }
    }
}