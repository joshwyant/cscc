using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class IntegerConstantAstNode : ConstantExpressionAstNode
    {
        public long Value { get; }
        public IntegerConstantAstNode(long value, int line, int column)
            : base(line, column)
        {
            Value = value;
        }
    }
}