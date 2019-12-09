using System.Collections.Generic;
using CParser.Lexing;
using System.Linq;

namespace tests
{
    static class Extensions
    {
        public static IEnumerable<Terminal> Terminals(this IEnumerable<Token> tokens)
        {
            return tokens.Select(t => t.Kind);
        }
    }
}