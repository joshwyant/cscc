using CParser.Lexing;

namespace CParser.Parsing.Ast
{
    public class DeclarationStatementAstNode : StatementAstNode
    {
        public DeclarationAstNode Declaration { get; }
        public DeclarationStatementAstNode(DeclarationAstNode d)
            : base(d.Line, d.Column)
        {
            Declaration = d;
        }
    }
}