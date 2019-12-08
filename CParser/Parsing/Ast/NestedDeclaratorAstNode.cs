namespace cscc.Parsing.Ast
{
    public class NestedDeclaratorAstNode : DeclaratorAstNode
    {
        public DeclaratorAstNode Declarator { get; }
        public override string? Name => Declarator.Name;
        public NestedDeclaratorAstNode(DeclaratorAstNode declarator,
            int line, int column)
            : base(line, column)
        {
            Declarator = declarator;
        }
    }
}