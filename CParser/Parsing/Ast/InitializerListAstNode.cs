using System.Collections.Generic;

namespace CParser.Parsing.Ast
{
    public class InitializerListAstNode : InitializerAstNode
    {
        public IReadOnlyList<InitializerAstNode>? List { get; }
        public InitializerListAstNode(
            IReadOnlyList<InitializerAstNode>? list, int line, int column)
            : base(line, column)
        {
            List = list;
        }
    }
}