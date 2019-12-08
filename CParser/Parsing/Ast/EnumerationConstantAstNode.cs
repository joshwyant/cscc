using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class EnumerationConstantAstNode : ConstantExpressionAstNode
    {
        public EnumSymbol Value { get; }
        public EnumerationConstantAstNode(EnumSymbol value, int line, int column)
            : base(line, column)
        {
            Value = value;
        }
    }
}