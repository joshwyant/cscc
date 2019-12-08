using System.Collections.Generic;
using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public abstract class StructOrUnionSpecifierAstNode : SpecifierAstNode
    {
        public string? Name { get; }
        public IReadOnlyList<StructDeclarationAstNode>? StructDeclarationList { get; }
        public StructOrUnionSpecifierAstNode(Terminal terminal,
            string? name,
            IReadOnlyList<StructDeclarationAstNode>? structDeclarationList, 
            int line, int column)
            : base(terminal, line, column)
        {
            Name = name;
            StructDeclarationList = structDeclarationList;
        }
    }
}