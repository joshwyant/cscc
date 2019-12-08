using System;
using CParser.Parsing.Ast;

namespace CParser.Translation
{
    class EnumSymbol : Symbol
    {
        public int Value { get; }

        public BasicSymbol(SymbolType type, string name)
            : base(type, name)
        {
            // Defer setting value until another pass of the compiler.
        }
    }
}