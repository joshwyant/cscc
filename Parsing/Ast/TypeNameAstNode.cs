using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    class TypeNameAstNode : AstNode
    {
        public IReadOnlyList<SpecifierAstNode> Specifiers { get; }
        public DeclaratorAstNode? Declarator { get; }
        public TypeNameAstNode(
            IReadOnlyList<SpecifierAstNode> specifiers,
            DeclaratorAstNode? declarator,
            int line, int column)
            : base(line, column)
        {
            Specifiers = specifiers;
            Declarator = declarator;
        }
    }
}