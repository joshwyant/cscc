using System.Collections.Generic;

namespace CParser.Parsing.Ast
{
    public class ExpressionListAstNode : ExpressionAstNode
    {
        public IReadOnlyList<ExpressionAstNode> List { get; }
        public ExpressionListAstNode(
            IReadOnlyList<ExpressionAstNode> list,
            int line, int column)
            : base(line, column)
        {
            List = list;
        }
    }
}