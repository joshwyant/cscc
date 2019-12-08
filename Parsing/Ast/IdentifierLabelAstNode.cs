namespace cscc.Parsing.Ast
{
    class IdentifierLabelAstNode : LabelAstNode
    {
        public string Name { get; }
        public IdentifierLabelAstNode(string name, int line, int column)
            : base(line, column)
        {
            Name = name;
        }
    }
}