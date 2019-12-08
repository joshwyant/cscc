using System.Collections.Generic;
using CParser.Lexing;
using CParser.Parsing;
using CParser.Parsing.Ast;
using CParser.Preprocessing;

namespace CParser.Translation
{
    public class Pipeline
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