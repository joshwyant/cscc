using System.Collections.Generic;
using cscc.Translation;
using cscc.Lexing;
using System.Linq;
using cscc.Parsing.Ast;

namespace cscc.Preprocessing
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