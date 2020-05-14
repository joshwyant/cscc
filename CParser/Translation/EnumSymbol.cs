using System;
using CParser.Parsing.Ast;

namespace CParser.Translation
{
    public class EnumSymbol : Symbol
    {
        public int Value { get; }

        public EnumSymbol(string name)
            : base(SymbolType.EnumSymbol, name)
        {
            // Defer setting value until another pass of the compiler.
        }
    }
}