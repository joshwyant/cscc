using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class EnumerationConstantAstNode : ConstantExpressionAstNode
    {
        public long Value { get; }
        public EnumerationConstantAstNode(long value, int line, int column)
            : base(line, column)
        {
            Value = value;
        }
    }
}