using System.Collections.Generic;
using System.Threading.Tasks;
using cscc.Helpers;
using cscc.Lexing;
using cscc.Parsing.Ast;
using cscc.Translation;

namespace cscc.Parsing
{
    class ExpressionParser : Parser
    {
        public ExpressionParser(TranslationUnit translationUnit, IStream<Token> inputStream)
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