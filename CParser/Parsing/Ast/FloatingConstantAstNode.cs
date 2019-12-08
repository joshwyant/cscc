using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class FloatingConstantAstNode : ConstantExpressionAstNode
    {
        public FloatingToken Value { get; }
        public FloatingConstantAstNode(FloatingToken value)
            : base(value.Line, value.Column)
        {
            Value = value;
        }
    }
}