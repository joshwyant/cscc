using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    public class FunctionDefinitionAstNode : DeclarationAstNode
    {
        public DeclaratorAstNode? Declarator { get; }
        public string? Name => Declarator?.Name;
        public IReadOnlyList<DeclarationAstNode> DeclarationList { get; }
        public CompoundStatementAstNode Body { get; }

        public FunctionDefinitionAstNode(
            IReadOnlyList<SpecifierAstNode>? specifiers, 
            DeclaratorAstNode? declarator, 
            IReadOnlyList<DeclarationAstNode> declarationList,
            CompoundStatementAstNode body, int line, int column)
            : base(specifiers, line, column)
        {
            Declarator = declarator;
            DeclarationList = declarationList;
            Body = body;
        }
    }
}