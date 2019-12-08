namespace cscc.Parsing.Ast
{
    class BitFieldDeclaratorAstNode : DeclaratorAstNode
    {
        public DeclaratorAstNode? Declarator { get; }
        public ExpressionAstNode BitPosition { get; }
        public override string? Name => Declarator?.Name;
        public BitFieldDeclaratorAstNode(DeclaratorAstNode? declarator,
            ExpressionAstNode bitPosition, int line, int column)
            : base(line, column)
        {
            Declarator = declarator;
            BitPosition = bitPosition;
        }
    }
}