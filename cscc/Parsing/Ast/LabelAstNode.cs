namespace cscc.Parsing.Ast
{
    public abstract class LabelAstNode : AstNode
    {
        public LabelAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}