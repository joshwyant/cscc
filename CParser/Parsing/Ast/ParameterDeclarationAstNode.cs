using System.Collections.Generic;

namespace CParser.Parsing.Ast
{
    public class ParameterDeclarationAstNode : AstNode
    {
        public IReadOnlyList<SpecifierAstNode>? Specifiers { get; }
        public DeclaratorAstNode Declarator { get; }
        public ParameterDeclarationAstNode(
            IReadOnlyList<SpecifierAstNode>? specifiers,
            DeclaratorAstNode declarator, int line, int column)
            : base(line, column)
        {
            Specifiers = specifiers;
            Declarator = declarator;
        }
    }
}