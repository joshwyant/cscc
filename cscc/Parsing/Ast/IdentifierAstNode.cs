using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    public class IdentifierAstNode : ExpressionAstNode
    {
        public string Identifier { get; }
        public IdentifierAstNode(string identifier, int line, int column)
            : base(line, column)
        {
            Identifier = identifier;
        }
    }
}