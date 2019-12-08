namespace CParser.Parsing.Ast
{
    public class BreakStatementAstNode : JumpStatementAstNode
    {
        public BreakStatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}