using System;

namespace cscc.Parsing.Ast
{
    public class InitDeclaratorAstNode : DeclaratorAstNode
    {
        DeclaratorAstNode Declarator { get; }
        public InitializerAstNode Initializer { get; }
        public override string Name => Declarator.Name!;
        public InitDeclaratorAstNode(DeclaratorAstNode declarator, 
            InitializerAstNode initializer,
            int line, int column)
            : base(line, column)
        {
            if (declarator.Name == null)
            {
                throw new InvalidOperationException(
                    "Can't use unnamed declarator with initializer.");
            }
            Declarator = declarator;
            Initializer = initializer;
        }
    }
}