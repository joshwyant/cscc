namespace CParser.Parsing.Ast
{
    public class IdentifierDeclaratorAstNode : DeclaratorAstNode
    {
        public override string Name { get; }
        public IdentifierDeclaratorAstNode(string name, int line, int column)
            : base(line, column)
        {
            Name = name;
        }
    }
}