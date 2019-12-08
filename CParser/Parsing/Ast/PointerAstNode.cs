using System.Collections.Generic;

namespace CParser.Parsing.Ast
{
    public class PointerAstNode : AstNode
    {
        public IReadOnlyList<TypeQualifierAstNode>? TypeQualifierList { get; }
        public PointerAstNode? OuterPointer { get; }
        public PointerAstNode(
            IReadOnlyList<TypeQualifierAstNode>? typeQualifierList,
            PointerAstNode? outerPointer, int line, int column)
            : base(line, column)
        {
            TypeQualifierList = typeQualifierList;
            OuterPointer = outerPointer;
        }
    }
}