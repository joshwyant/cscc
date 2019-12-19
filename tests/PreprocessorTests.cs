using System;
using Xunit;
using CParser.Preprocessing;
using CParser.Translation;
using System.IO;
using System.Linq;
using CParser.Helpers;
using static CParser.Lexing.Terminal;

namespace tests
{
    public class PreprocessorTests
    {
        protected Preprocessor CreateTestPreprocessor(string program, bool preprocess = true)
        {
            return new Preprocessor(
                new TranslationUnit(new FakeFileResolver(), "test.c"), 
                new StringReader(program), 
                preprocess);
        }

        [Fact]
        public async void TestTokenSequence()
        {
            var program = 
                @"int main() {
                    printf(""Hello, world!\n"");
                }";
            var preprocessor = CreateTestPreprocessor(program, true);
            var tokens = (await preprocessor.AsList());
            Assert.Equal(new[] { 
                    Int, Identifier, LeftParen, RightParen, LeftBrace,
                    Identifier, LeftParen, StringLiteral, RightParen, Semicolon,
                    RightBrace,
                    Eof
                }, tokens.Terminals());
            Assert.Equal(new[] { 
                    "int", "main", "(", ")", "{",
                    "printf", "(", "Hello, world!\n", ")", ";",
                    "}",
                    "end-of-file"
                }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async void TestIncludes()
        {
            var program = 
                @"#include ""a.h""
                #include ""b.h""
                int main() {
                    printf(""Hello, world!\n"");
                }";
            var preprocessor = CreateTestPreprocessor(program, true);
            var resolver = preprocessor.TranslationUnit.FileResolver as FakeFileResolver;
            resolver.DefineFile("a.h", @"int someFuncA();");
            resolver.DefineFile("b.h", @"int someFuncB();");
            var tokens = (await preprocessor.AsList());
            Assert.Equal(new[] { 
                    Int, Identifier, LeftParen, RightParen, Semicolon,
                    Int, Identifier, LeftParen, RightParen, Semicolon,
                    Int, Identifier, LeftParen, RightParen, LeftBrace,
                    Identifier, LeftParen, StringLiteral, RightParen, Semicolon,
                    RightBrace,
                    Eof
                }, tokens.Terminals());
            Assert.Equal(new[] { 
                    "int", "someFuncA", "(", ")", ";",
                    "int", "someFuncB", "(", ")", ";",
                    "int", "main", "(", ")", "{",
                    "printf", "(", "Hello, world!\n", ")", ";",
                    "}",
                    "end-of-file"
                }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async void TestNestedIncludes()
        {
            var program = 
                @"#include ""a.h""
                #include ""b.h""
                int main() {
                    printf(""Hello, world!\n"");
                }";
            var preprocessor = CreateTestPreprocessor(program, true);
            var resolver = preprocessor.TranslationUnit.FileResolver as FakeFileResolver;
            resolver.DefineFile("a.h", @"int someFuncA();");
            resolver.DefineFile("b.h", "int someFuncB();\n#include \"c.h\"");
            resolver.DefineFile("c.h", "int someFuncC();");
            var tokens = (await preprocessor.AsList());
            Assert.Equal(new[] { 
                    Int, Identifier, LeftParen, RightParen, Semicolon,
                    Int, Identifier, LeftParen, RightParen, Semicolon,
                    Int, Identifier, LeftParen, RightParen, Semicolon,
                    Int, Identifier, LeftParen, RightParen, LeftBrace,
                    Identifier, LeftParen, StringLiteral, RightParen, Semicolon,
                    RightBrace,
                    Eof
                }, tokens.Terminals());
            Assert.Equal(new[] { 
                    "int", "someFuncA", "(", ")", ";",
                    "int", "someFuncB", "(", ")", ";",
                    "int", "someFuncC", "(", ")", ";",
                    "int", "main", "(", ")", "{",
                    "printf", "(", "Hello, world!\n", ")", ";",
                    "}",
                    "end-of-file"
                }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async void TestEmptyDefine()
        {
            var program = 
                @"#define t
                t u";
            var preprocessor = CreateTestPreprocessor(program, true);
            var tokens = (await preprocessor.AsList());
            Assert.Equal(new[] { 
                    Identifier,
                    Eof
                }, tokens.Terminals());
            Assert.Equal(new[] { 
                    "u",
                    "end-of-file"
                }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async void TestDefineTokens()
        {
            var program = 
                @"#define t x y z 1 'a'
                t u";
            var preprocessor = CreateTestPreprocessor(program, true);
            var tokens = (await preprocessor.AsList());
            Assert.Equal(new[] { 
                    Identifier, Identifier, Identifier, IntegerConstant, CharLiteral, Identifier,
                    Eof
                }, tokens.Terminals());
            Assert.Equal(new[] { 
                    "x", "y", "z", "1", "a", "u",
                    "end-of-file"
                }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async void TestContinuingDefineTokens()
        {
            var program = 
                @"#define t a \
                b
                t u t";
            var preprocessor = CreateTestPreprocessor(program, true);
            var tokens = (await preprocessor.AsList());
            Assert.Equal(new[] { 
                    Identifier, Identifier, Identifier, Identifier, Identifier,
                    Eof
                }, tokens.Terminals());
            Assert.Equal(new[] { 
                    "a", "b", "u", "a", "b",
                    "end-of-file"
                }, tokens.Select(t => t.ToString()));
        }
    }
}
