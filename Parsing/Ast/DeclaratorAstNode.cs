namespace cscc.Parsing.Ast
{
    abstract class DeclaratorAstNode : AstNode
    {
        public abstract string? Name { get; }
        public DeclaratorAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}