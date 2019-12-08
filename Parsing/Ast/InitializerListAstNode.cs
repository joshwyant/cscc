using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    class InitializerListAstNode : InitializerAstNode
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