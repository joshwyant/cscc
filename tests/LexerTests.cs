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
                new TranslationUnit("test.c"), 
                new CharacterStream(new StringReader(program)), 
                preprocess, preprocess);
        }

        [Fact]
        public async void Test1()
        {
            var program = 
                @"int main() {
                    printf(""Hello, world!\n"");
                }";
            var lexer = CreateTestLexer(program, false);
            Assert.Equal(new[] { 
                    Int, Identifier, LeftParen, RightParen, LeftBrace,
                    Identifier, LeftParen, StringLiteral, RightParen, Semicolon,
                    RightBrace,
                    Eof
                }, (await lexer.AsEnumerable()).Terminals());
        }
    }
}
