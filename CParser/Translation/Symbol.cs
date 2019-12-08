using System;
using CParser.Parsing.Ast;

namespace CParser.Translation
{
    abstract class Symbol
    {
        public SymbolType Type { get; }
        public string Name { get; }

        public Symbol(SymbolType type, string name)
        {
            Type = type;
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }
    }
}