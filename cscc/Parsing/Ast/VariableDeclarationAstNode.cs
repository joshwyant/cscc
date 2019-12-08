using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    public class VariableDeclarationAstNode : DeclarationAstNode
    {
        public IReadOnlyList<DeclaratorAstNode>? DeclaratorList { get; }
        public VariableDeclarationAstNode(IReadOnlyList<SpecifierAstNode> specifiers,
            IReadOnlyList<DeclaratorAstNode>? declaratorList, int line, 
            int column)
            : base(specifiers, line, column)
        {
            DeclaratorList = declaratorList;
        }
    }
}