using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class IntegerConstantAstNode : ConstantExpressionAstNode
    {
        public IntegerToken Value { get; }
        public IntegerConstantAstNode(IntegerToken value)
            : base(value.Line, value.Column)
        {
            Value = value;
        }
    }
}