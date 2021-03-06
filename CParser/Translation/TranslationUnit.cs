using System.Collections.Generic;
using CParser.Lexing;
using static CParser.Lexing.LexerState;

namespace CParser.Translation
{
    public class TranslationUnit
    {
        public FileResolver FileResolver { get; }
        public SymbolTable Defines { get; }

        public SymbolTable Symbols { get; set; }

        public SymbolTable Tags { get; }

        public SymbolTable Labels { get; set; }

        public LexerState LexerState { get; set; } = LexerReady;

        public int CurrentLine { get; set; } = 1;

        public int CurrentColumn { get; set; } = 1;

        public string CurrentFilename { get; set; }
        public List<CompileError> Errors { get; }

        public TranslationUnit(FileResolver fileResolver, string currentFilename)
        {
            FileResolver = fileResolver;
            Defines = new SymbolTable();
            Symbols = new SymbolTable();
            Tags = new SymbolTable();
            Labels = new SymbolTable();
            Errors = new List<CompileError>();
            CurrentFilename = currentFilename;
        }
    }
}