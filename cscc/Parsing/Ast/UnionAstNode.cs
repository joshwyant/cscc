using System.Collections.Generic;
using cscc.Lexing;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
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