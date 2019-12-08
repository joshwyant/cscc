using System.Collections.Generic;
using CParser.Translation;
using CParser.Lexing;
using System.Linq;
using CParser.Parsing.Ast;

namespace CParser.Preprocessing
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