using System.Collections.Generic;
using static CParser.Lexing.Terminal;

namespace CParser.Parsing.Ast
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