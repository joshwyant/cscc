namespace cscc.Parsing.Ast
{
    class ForStatementAstNode : StatementAstNode
    {
        public ExpressionAstNode? InitializationExpression { get; }
        public ExpressionAstNode? ConditionExpression { get; }
        public ExpressionAstNode? AfterthoughtExpression { get; }
        public StatementAstNode Statement { get; }
        public ForStatementAstNode(
            ExpressionAstNode? initializationExpression,
            ExpressionAstNode? conditionExpression,
            ExpressionAstNode? afterthoughtExpression,
            StatementAstNode statement,
            int line, int column)
            : base(line, column)
        {
            InitializationExpression = initializationExpression;
            ConditionExpression = conditionExpression;
            AfterthoughtExpression = afterthoughtExpression;
            Statement = statement;
        }
    }
}