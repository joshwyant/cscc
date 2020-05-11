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
            var program = @"#include ""stdio.h""
                            main() {
                                printf(""Hello, world!\n"");
                            }";
            var parser = CreateTestParser(program);
            var resolver = parser.TranslationUnit.FileResolver as FakeFileResolver;
            resolver.DefineFile("stdio.h", "void printf();");
            var results = await parser.AsList();

            // 1: void printf();
            VariableDeclarationAstNode printfDecl;
            Assert.Equal(2, results.Count);
            Assert.NotNull(printfDecl = results[0] as VariableDeclarationAstNode);
            Assert.Equal(1, printfDecl.DeclaratorList.Count);
            Assert.Equal("printf", printfDecl.DeclaratorList[0].Name);

            // 2: main() {
            FunctionDefinitionAstNode printfDef;
            Assert.NotNull(printfDef = results[1] as FunctionDefinitionAstNode);
            Assert.Equal("main", printfDef.Name);

            // 3: printf(""Hello, world!\n"");
            PostfixCallExpressionAstNode callExpr;
            IdentifierAstNode ident;
            StringLiteralAstNode str;
            Assert.Equal(1, printfDef.Body.StatementList.Count);
            Assert.NotNull(printfDef.Body.StatementList[0] as ExpressionStatementAstNode);
            Assert.NotNull(callExpr = (printfDef.Body.StatementList[0] as ExpressionStatementAstNode).Expression as PostfixCallExpressionAstNode);
            Assert.NotNull(ident = callExpr.Function as IdentifierAstNode);
            Assert.Equal("printf", ident.Identifier);
            Assert.Equal(1, callExpr.Parameters.Count);
            Assert.NotNull(str = callExpr.Parameters[0] as StringLiteralAstNode);
            Assert.Equal("Hello, world!\n", str.Text);
        }
    }
}