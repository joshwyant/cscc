using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class FloatingConstantAstNode : ConstantExpressionAstNode
    {
        public decimal Value { get; }
        public FloatingConstantAstNode(decimal value, int line, int column)
            : base(line, column)
        {
            Value = value;
        }
    }
}