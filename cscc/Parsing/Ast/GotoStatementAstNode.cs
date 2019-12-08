namespace cscc.Parsing.Ast
{
    public class GotoStatementAstNode : JumpStatementAstNode
    {
        public string Label { get; }
        public GotoStatementAstNode(string label, int line, int column)
            : base(line, column)
        {
            Label = label;
        }
    }
}