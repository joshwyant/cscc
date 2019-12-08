namespace CParser.Parsing.Ast
{
    public class ParameterizedDeclaratorAstNode : DeclaratorAstNode
    {
        public DeclaratorAstNode? Declarator { get; }
        public ParameterTypeListAstNode ParameterTypeList { get; }
        public override string? Name => Declarator?.Name;
        public ParameterizedDeclaratorAstNode(
            DeclaratorAstNode? declarator, 
            ParameterTypeListAstNode parameterTypeList,
            int line, int column)
            : base(line, column)
        {
            Declarator = declarator;
            ParameterTypeList = parameterTypeList;
        }
    }
}