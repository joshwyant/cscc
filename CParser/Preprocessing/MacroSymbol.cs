using System.Collections.Generic;
using cscc.Translation;
using cscc.Lexing;
using System.Linq;
using cscc.Parsing.Ast;

namespace cscc.Preprocessing
{
    class MacroSymbol : Symbol
    {
        public IReadOnlyList<Token> Parameters { get; }
        public IReadOnlyList<Token> Definition { get; }
        public MacroSymbol(SymbolType type, string name, IReadOnlyList<Token> parameters, IReadOnlyList<Token> definition) 
            : base(type, name)
        {
            Parameters = parameters;
            Definition = definition;
        }
    }
}