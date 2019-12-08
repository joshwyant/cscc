using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class CharacterConstantAstNode : ConstantExpressionAstNode
    {
        public string Value { get; }
        public CharacterConstantAstNode(string value, int line, int column)
            : base(line, column)
        {
            Value = value;
        }
    }
}