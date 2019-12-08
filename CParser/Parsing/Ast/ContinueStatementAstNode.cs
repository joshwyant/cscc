namespace CParser.Parsing.Ast
{
    public class ContinueStatementAstNode : JumpStatementAstNode
    {
        public ContinueStatementAstNode(int line, int column)
            : base(line, column)
        {
        }
    }
}