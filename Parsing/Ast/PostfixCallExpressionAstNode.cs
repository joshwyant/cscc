using System.Collections.Generic;
using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class PostfixCallExpressionAstNode : ExpressionAstNode
    {
        public ExpressionAstNode Function { get; }
        public IReadOnlyList<ExpressionAstNode>? Parameters { get; }
        public PostfixCallExpressionAstNode(
            ExpressionAstNode function, 
            IReadOnlyList<ExpressionAstNode>? parameters,
            int line, int column)
            : base(line, column)
        {
            Function = function;
            Parameters = parameters;
        }
    }
}