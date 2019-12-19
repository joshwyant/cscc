using System.IO;
using CParser.Helpers;
using CParser.Parsing;
using CParser.Parsing.Ast;
using CParser.Preprocessing;
using CParser.Translation;
using Xunit;

namespace tests
{
    public class ParserTests
    {
        protected Parser CreateTestParser(string program)
        {
            var tu = new TranslationUnit(new FakeFileResolver(), "test.c");
            return new Parser(
                tu, new Preprocessor(tu, new StringReader(program), true));
        }

        [Fact]
        public async void TestBasicC89Program()
        {
            var program = @"#include <stdio.h>
                            main() {
                                printf(""Hello, world!\n"");
                            }";
            var parser = CreateTestParser(program);
            var resolver = parser.TranslationUnit.FileResolver as FakeFileResolver;
            resolver.DefineFile("stdio.h", "void printf();");
            var results = await parser.AsList();
            FunctionDefinitionAstNode printf;
            Assert.NotNull(printf = results[0] as FunctionDefinitionAstNode);
            Assert.Equal("printf", printf.Name);
        }
    }
}