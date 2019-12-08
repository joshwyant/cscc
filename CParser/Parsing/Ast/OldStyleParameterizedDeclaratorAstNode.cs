using System.Collections.Generic;

namespace CParser.Parsing.Ast
{
    public class OldStyleParameterizedDeclaratorAstNode : DeclaratorAstNode
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