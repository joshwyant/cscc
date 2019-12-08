namespace CParser.Parsing.Ast
{
    public class PointerDeclaratorAstNode : DeclaratorAstNode
    {
        public PointerAstNode Pointer { get; }
        public DeclaratorAstNode? Declarator { get; }
        public override string? Name => Declarator?.Name;
        public PointerDeclaratorAstNode(PointerAstNode pointer,
            DeclaratorAstNode? declarator, int line, int column)
            : base(line, column)
        {
            Pointer = pointer;
            Declarator = declarator;
        }
    }
}