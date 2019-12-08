using System;
using cscc.Parsing.Ast;

namespace cscc.Translation
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