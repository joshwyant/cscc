using System.Collections.Generic;

namespace CParser.Parsing.Ast
{
    public abstract class DeclarationAstNode : AstNode
    {
        public IReadOnlyList<SpecifierAstNode>? Specifiers { get; }
        public DeclarationAstNode(IReadOnlyList<SpecifierAstNode>? specifiers,
            int line, int column)
            : base(line, column)
        {
            Specifiers = specifiers;
        }
    }
}