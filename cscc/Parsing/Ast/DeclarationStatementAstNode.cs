using cscc.Lexing;

namespace cscc.Parsing.Ast
{
    class DeclarationStatementAstNode : StatementAstNode
    {
        public DeclarationAstNode Declaration { get; }
        public DeclarationStatementAstNode(DeclarationAstNode d)
            : base(d.Line, d.Column)
        {
            Declaration = d;
        }
    }
}