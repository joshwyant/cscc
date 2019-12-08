using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class ExpressionStatementAstNode : StatementAstNode
    {
        // Optional for empty statement (semicolon only)
        public ExpressionAstNode? Expression { get; }
        public ExpressionStatementAstNode(ExpressionAstNode? e, int line, int column)
            : base(line, column)
        {
            Expression = e;
        }
    }
}