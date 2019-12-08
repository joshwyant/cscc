namespace cscc.Parsing.Ast
{
    class LabeledStatementAstNode : StatementAstNode
    {
        public LabelAstNode Label { get; }
        public StatementAstNode Statement { get; }
        public LabeledStatementAstNode(
            LabelAstNode label,
            StatementAstNode statement,
            int line, int column)
            : base(line, column)
        {
            Label = label;
            Statement = statement;
        }
    }
}