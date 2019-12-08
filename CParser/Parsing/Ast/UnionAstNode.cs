using System.Collections.Generic;
using CParser.Lexing;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
{
    public class UnionAstNode : StructOrUnionSpecifierAstNode
    {
        public UnionAstNode(
            string? name, 
            IReadOnlyList<StructDeclarationAstNode>? structDeclarationList,
            int line, int column)
            : base(Struct, name, structDeclarationList, line, column)
        {
        }
    }
}