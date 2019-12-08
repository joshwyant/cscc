using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class ConditionalExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode Condition { get; }
        public ExpressionAstNode TrueExpression { get; }
        public ExpressionAstNode FalseExpression { get; }
        public ConditionalExpressionAstNode(ExpressionAstNode condition,
            ExpressionAstNode trueExpression, 
            ExpressionAstNode faleExpression,
            int line, int column)
            : base(line, column)
        {
            Condition = condition;
            TrueExpression = trueExpression;
            FalseExpression = FalseExpression;
        }
    }
}