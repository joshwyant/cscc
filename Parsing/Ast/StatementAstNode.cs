namespace cscc.Parsing.Ast
{
    abstract class StatementAstNode : AstNode
    {
        public StatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}