using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    class OldStyleParameterizedDeclaratorAstNode : DeclaratorAstNode
    {
        public DeclaratorAstNode Declarator { get; }
        public IReadOnlyList<string>? ParameterNameList { get; }
        public override string? Name => Declarator.Name;
        public OldStyleParameterizedDeclaratorAstNode(
            DeclaratorAstNode declarator, 
            IReadOnlyList<string>? parameterNameList,
            int line, int column)
            : base(line, column)
        {
            Declarator = declarator;
            ParameterNameList = parameterNameList;
        }
    }
}