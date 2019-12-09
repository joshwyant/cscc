using System;
using Xunit;
using CParser.Lexing;
using CParser.Translation;
using System.IO;
using System.Linq;
using CParser.Helpers;
using static CParser.Lexing.Terminal;

namespace tests
{
    public class LexerTests
    {
        protected Lexer CreateTestLexer(string program, bool preprocess = true)
        {
            return new Lexer(
                new TranslationUnit(new FileResolver(), "test.c"), 
                new CharacterStream(new StringReader(program)), 
                preprocess, preprocess);
        }

        [Fact]
        public async void TestTokenSequence()
        {
            var program = 
                @"int main() {
                    printf(""Hello, world!\n"");
                }";
            var lexer = CreateTestLexer(program, false);
            var tokens = (await lexer.AsEnumerable()).ToList();
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
    }
}
