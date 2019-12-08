namespace cscc.Parsing.Ast
{
    abstract class LabelAstNode : AstNode
    {
        public LabelAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}