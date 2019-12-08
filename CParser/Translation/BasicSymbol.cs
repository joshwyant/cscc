using System;
using CParser.Parsing.Ast;

namespace CParser.Translation
{
    class BasicSymbol : Symbol
    {
        public AstNode Value { get; }

        public BasicSymbol(SymbolType type, string name, AstNode value)
            : base(type, name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            Value = value;
        }
    }
}