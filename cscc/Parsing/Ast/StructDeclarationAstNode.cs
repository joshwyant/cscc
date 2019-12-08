using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    class StructDeclarationAstNode : DeclarationAstNode
    {
        IReadOnlyList<DeclaratorAstNode>? StructDeclaratorList { get; }
        public StructDeclarationAstNode(
            IReadOnlyList<SpecifierAstNode> specifierQualifierList,
            IReadOnlyList<DeclaratorAstNode>? structDeclaratorList,
            int line, int column)
            : base(specifierQualifierList, line, column)
        {
            StructDeclaratorList = structDeclaratorList;
        }
    }
}