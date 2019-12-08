using System.Collections.Generic;
using CParser.Translation;
using CParser.Lexing;
using System.Linq;
using CParser.Parsing.Ast;

namespace CParser.Preprocessing
{
    class DefineSymbol : Symbol
    {
        public IReadOnlyList<Token> Definition { get; }
        public DefineSymbol(SymbolType type, string name, IReadOnlyList<Token> definition) 
            : base(type, name)
        {
            Definition = definition;
        }
    }
}