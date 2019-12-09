using System.Collections.Generic;
using System.Threading.Tasks;
using CParser.Helpers;
using CParser.Lexing;
using CParser.Parsing.Ast;
using CParser.Translation;

namespace CParser.Parsing
{
    class ExpressionParser : Parser
    {
        public ExpressionParser(TranslationUnit translationUnit, IAsyncStream<Token> inputStream)
            : base(translationUnit, inputStream)
        {
        }

        protected override async IAsyncEnumerable<AstNode> Parse()
        {
            var e = await expression();
            if (e != default)
            {
                yield return e;
            }
        }
    }
}