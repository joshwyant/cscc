using System.Collections.Generic;
using CParser.Lexing;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
{
    public class StructAstNode : StructOrUnionSpecifierAstNode
    {
        public StructAstNode(
            string? name,
            IReadOnlyList<StructDeclarationAstNode>? structDeclarationList,
            int line, int column)
            : base(Struct, name, structDeclarationList, line, column)
        {
        }
    }
}