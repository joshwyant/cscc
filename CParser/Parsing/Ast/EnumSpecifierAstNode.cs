using System.Collections.Generic;
using static cscc.Lexing.Terminal;

namespace cscc.Parsing.Ast
{
    public class EnumSpecifierAstNode : SpecifierAstNode
    {
        IReadOnlyList<EnumeratorAstNode>? EnumeratorList { get; }
        public EnumSpecifierAstNode(string? name, 
            IReadOnlyList<EnumeratorAstNode>? enumeratorList, 
            int line, int column)
            : base(Enum, line, column)
        {
            EnumeratorList = enumeratorList;
        }
    }
}