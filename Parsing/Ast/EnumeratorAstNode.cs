namespace cscc.Parsing.Ast
{
    class EnumeratorAstNode : AstNode
    {
        public string Name { get; }
        public ExpressionAstNode? Value { get; }
        public EnumeratorAstNode(string name, ExpressionAstNode? value, 
            int line, int column)
            : base(line, column)
        {
            Name = name;
            Value = value;
        }
    }
}