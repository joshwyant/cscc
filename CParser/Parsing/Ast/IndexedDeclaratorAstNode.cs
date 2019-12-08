namespace cscc.Parsing.Ast
{
    public class IndexedDeclaratorAstNode : DeclaratorAstNode
    {
        public DeclaratorAstNode? Declarator { get; }
        public ExpressionAstNode Index { get; }
        public override string? Name => Declarator?.Name;
        public IndexedDeclaratorAstNode(DeclaratorAstNode? declarator,
            ExpressionAstNode index,
            int line, int column)
            : base(line, column)
        {
            Declarator = declarator;
            Index = index;
        }
    }
}