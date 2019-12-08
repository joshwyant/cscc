using System.Collections.Generic;
using cscc.Lexing;
using cscc.Parsing;
using cscc.Parsing.Ast;
using cscc.Preprocessing;

namespace cscc.Translation
{
    class Pipeline
    {
        public FileResolver FileResolver { get; }

        public Pipeline(FileResolver fileResolver)
        {
            FileResolver = fileResolver;
        }

        public IAsyncEnumerable<AstNode> ParseFile(string filename, bool preprocess)
        {
            var tu = new TranslationUnit(filename);
            var reader = FileResolver.ResolveTextReader(filename);
            var preprocessor = new Preprocessor(tu, reader, preprocess);
            var parser = new Parser(tu, preprocessor);
            return parser;
        }
    }
}