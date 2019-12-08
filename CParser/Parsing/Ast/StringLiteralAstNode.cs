using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class StringLiteralAstNode : ExpressionAstNode
    {
        public string Text { get; }
        public StringLiteralAstNode(string text, int line, int column)
            : base(line, column)
        {
            Text = text;
        }
    }
}