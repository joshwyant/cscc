using System;
using System.Threading.Tasks;
using Xunit;
using CParser.Lexing;
using CParser.Translation;
using System.IO;
using System.Linq;
using CParser.Helpers;
using static CParser.Lexing.Terminal;
using System.Collections.Generic;
using CParser.Parsing.Ast;

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
        public async Task TestTokenSequence()
        {
            var program = 
                @"int main() {
                    printf(""Hello, world!\n"");
                }";
            var lexer = CreateTestLexer(program, false);

            var tokens = await lexer.AsList();

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
        public async Task TestTokenSequenceBlock()
        {
            var program = 
                @"int main() {
                    printf(""Hello, world!\n"");
                }";
            var lexer = Lexer.AsBlock(new TranslationUnit(new FileResolver(), "test.c"), false, false);

            await lexer.PostAllTextAsync(new StringReader(program));
            lexer.Complete();
            var tokens = (await lexer.ReceiveAllAsync().AsList());

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
        public async Task TestKeywords()
        {
            var program = @" auto asm break case char const continue default do double else enum "
                + "extern float for goto if int long register return short signed sizeof static "
                + "struct switch typedef union unsigned void volatile while ";
            var lexer = CreateTestLexer(program, false);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                Auto, Asm, Break, Case, Terminal.Char, Const, Continue, Default,
                Do, Terminal.Double, Else, Terminal.Enum, Extern, Terminal.Float,
                For, Goto, If, Int, Terminal.Long, Register, Return, Terminal.Short, Signed,
                Sizeof, Static, Struct, Switch, Typedef, Union, Unsigned, Terminal.Void, Volatile,
                While, Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "auto", "asm", "break", "case", "char", "const", "continue", "default", "do",
                "double", "else", "enum", "extern", "float", "for", "goto", "if", "int", "long",
                "register", "return", "short", "signed", "sizeof", "static", "struct",
                "switch", "typedef", "union", "unsigned", "void", "volatile", "while",
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestEmptyFile()
        {
            var program = @"";
            var lexer = CreateTestLexer(program, false);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestFilenameMode()
        {
            var program = @"<test.c>";
            var lexer = CreateTestLexer(program, false);
            var enumerator = lexer.GetAsyncEnumerator();
            var tokens = new List<Token>();
            
            // <
            Assert.True(await enumerator.MoveNextAsync());
            tokens.Add(enumerator.Current);
            // test.c
            lexer.TranslationUnit.LexerState = LexerState.LexingLibraryFilename;
            Assert.True(await enumerator.MoveNextAsync());
            tokens.Add(enumerator.Current);
            // >
            Assert.True(await enumerator.MoveNextAsync());
            tokens.Add(enumerator.Current);
            // end-of-file
            Assert.True(await enumerator.MoveNextAsync());
            tokens.Add(enumerator.Current);
            Assert.False(await enumerator.MoveNextAsync());

            Assert.Equal(new[] {
                LessThan, Filename, GreaterThan, Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "<", "test.c", ">",
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestIdentifierTypes()
        {
            var program = @"ident typedef_name enum_constant";
            var lexer = CreateTestLexer(program, false);
            lexer.TranslationUnit.Symbols
                .Add(new KeyValuePair<string, Symbol>("typedef_name", 
                    new BasicSymbol(SymbolType.TypedefSymbol, "typedef_name", new FakeAstNode())));
            lexer.TranslationUnit.Symbols
                .Add(new KeyValuePair<string, Symbol>("enum_constant", 
                    new EnumSymbol("enum_constant")));

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                Identifier, TypedefName, EnumConstant,
                Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "ident", "typedef_name", "enum_constant",
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestNumbers()
        {
            var program = @"0x1234567890abcdef 0XAaBbCcDdEeFf 0xaAu 0xaAl 0xaAul 0xaALU "
                + "0777 077u 077l 077ul 077LU "
                + "88 88u 88l 88ul 88LU "
                + "1e0 1e10 1E+10 1e-10 "
                + "0.33 11.0 11.33 11.33e+10 11.33E-10 "
                + "0.33f 11.0F 11.33f 11.33e+10F 11.33E-10f "
                + "1.7976931348623157e308 4.94065645841247e-324 "
                + "1.5e309 1.5e-325 1.5e308 5e-324";
            var lexer = CreateTestLexer(program, false);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                IntegerConstant, IntegerConstant, IntegerConstant, IntegerConstant, 
                IntegerConstant, IntegerConstant, IntegerConstant, IntegerConstant, 
                IntegerConstant, IntegerConstant, IntegerConstant, IntegerConstant, 
                IntegerConstant, IntegerConstant, IntegerConstant, IntegerConstant, 
                FloatingConstant, FloatingConstant, FloatingConstant, FloatingConstant, 
                FloatingConstant, FloatingConstant, FloatingConstant, FloatingConstant, 
                FloatingConstant, FloatingConstant, FloatingConstant, FloatingConstant, 
                FloatingConstant, FloatingConstant, FloatingConstant, FloatingConstant,
                FloatingConstant, FloatingConstant, FloatingConstant, FloatingConstant,  
                Eof
            }, tokens.Terminals());
            var ints = tokens.Take(16).Cast<IntegerToken>().ToList();
            var floats = tokens.Skip(16).Take(20).Cast<FloatingToken>().ToList();
            Assert.Equal(new[] {
                0x1234567890abcdeful, 0xaabbccddeefful, 0xaaul, 0xaaul, 0xaaul, 0xaaul, 
                Convert.ToUInt64("777", 8), Convert.ToUInt64("77", 8), Convert.ToUInt64("77", 8),
                Convert.ToUInt64("77", 8), Convert.ToUInt64("77", 8),
                88ul, 88ul, 88ul, 88ul, 88ul
            }, ints.Select(i => i.Value));
            Assert.Equal(new[] {
                false, false, true, false, true, true, 
                false, true, false, true, true,
                false, true, false, true, true
            }, ints.Select(i => i.HasUnsignedSpecifier));
            Assert.Equal(new[] {
                false, false, false, true, true, true,
                false, false, true, true, true,
                false, false, true, true, true
            }, ints.Select(i => i.HasLongSpecifier));
            Assert.Equal(1e0, floats[0].Value, 11);
            Assert.Equal(1e10, floats[1].Value, 11);
            Assert.Equal(1e10, floats[2].Value, 11);
            Assert.Equal(1e-10, floats[3].Value, 11);
            Assert.Equal(0.33, floats[4].Value, 11);
            Assert.Equal(11, floats[5].Value, 11);
            Assert.Equal(11.33, floats[6].Value, 11);
            Assert.Equal(11.33e10, floats[7].Value, 11);
            Assert.Equal(11.33e-10, floats[8].Value, 11);
            Assert.Equal(0.33, floats[9].Value, 11);
            Assert.Equal(11, floats[10].Value, 11);
            Assert.Equal(11.33, floats[11].Value, 11);
            Assert.Equal(11.33e10, floats[12].Value, 11);
            Assert.Equal(11.33e-10, floats[13].Value, 11);
            Assert.Equal(double.MaxValue, floats[14].Value, 11);
            Assert.Equal(double.Epsilon, floats[15].Value, 11);
            Assert.Equal(double.MaxValue, floats[16].Value, 11);
            Assert.Equal(double.Epsilon, floats[17].Value, 11);
            Assert.Equal(1.5e308, floats[18].Value, 11);
            Assert.Equal(5e-324, floats[19].Value, 11);
            Assert.Equal(new[] {
                false, false, false, false,
                false, false, false, false, false,
                true, true, true, true, true,
                false, false, false, false, false, false
            }, floats.Select(f => f.HasFloatSpecifier));
        }

        [Fact]
        public async Task Test()
        {
            var program = "   \t\r\t\r\n\t\n\n";
            var lexer = new Lexer(
                new TranslationUnit(new FileResolver(), "test.c"), 
                new CharacterStream(new StringReader(program)), 
                false, true);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                Whitespace, Newline, Whitespace, Newline, Whitespace, Newline, Newline,
                Eof
            }, tokens.Terminals());
        }

        [Fact]
        public async Task TestComments()
        {
            var program = @"// Hello,
                            /* World! */";
            var lexer = new Lexer(
                new TranslationUnit(new FileResolver(), "test.c"), 
                new CharacterStream(new StringReader(program)), 
                false, true);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                Comment, Newline, Whitespace, Comment,
                Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "Hello,", "World!"
            }, tokens.Where(t => t.Kind == Comment).Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestStringsAndChars()
        {
            var program = @"
                ""Hello \10\xa1\XF9\r\n\v\b\f\a\\\?\'\""\0""
                'a' '\10' '\xa1' '\XF9' '\r' '\n' '\v'
                '\b' '\f' '\a' '\\' '\?' '\'' '\""' '\0'
            ";
            var lexer = CreateTestLexer(program, false);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                StringLiteral, CharLiteral, CharLiteral, CharLiteral, CharLiteral,
                CharLiteral, CharLiteral, CharLiteral, CharLiteral, CharLiteral,
                CharLiteral, CharLiteral, CharLiteral, CharLiteral, CharLiteral,
                CharLiteral,
                Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "Hello \x8\xa1\xf9\r\n\v\b\f\a\\?'\"\0",
                "a", "\x8", "\xa1", "\xf9", "\r", "\n", "\v",
                "\b", "\f", "\a", "\\", "?", "'", "\"", "\0",
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestPunctuation()
        {
            var program = @"~###(){}[]:;,.?!=!%=%^=^&&&=&*=*--->-=-+++=+ ===|||=|"
                + "<<<<=<=<>>>>=>=>/=/`@\\";
            var lexer = CreateTestLexer(program, false);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                Tilde, Unknown, Unknown, Unknown, LeftParen, RightParen, LeftBrace, RightBrace,
                LeftBracket, RightBracket, Colon, Semicolon, Comma, Dot, Query,
                NotEqual, Bang, ModAssign, Percent, XorAssign, Caret, LogicalAnd,
                AndAssign, Ampersand, MultiplyAssign, Star, Decrement, Arrow,
                SubtractAssign, Minus, Increment, AddAssign, Plus, DoubleEquals, Assign,
                LogicalOr, OrAssign, Pipe, ShiftLeft, ShiftLeftAssign, LessThanOrEqual,
                LessThan, ShiftRight, ShiftRightAssign, GreaterThanOrEqual, GreaterThan,
                DivideAssign, Slash, Unknown, Unknown, Unknown,
                Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "~", "#", "#", "#", "(", ")", "{", "}", "[", "]", ":", ";", ",",
                ".", "?", "!=", "!", "%=", "%", "^=", "^", "&&", "&=", "&", "*=",
                "*", "--", "->", "-=", "-", "++", "+=", "+", "==", "=", "||", "|=",
                "|", "<<", "<<=", "<=", "<", ">>", ">>=", ">=", ">", "/=", "/", "`", "@", "\\",
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        [Fact]
        public async Task TestPreprocessorPunctuation()
        {
            var program = @"###";
            var lexer = CreateTestLexer(program, true);

            var tokens = await lexer.AsList();

            Assert.Equal(new[] {
                DoublePound, Pound,
                Eof
            }, tokens.Terminals());
            Assert.Equal(new[] {
                "##", "#",
                "end-of-file"
            }, tokens.Select(t => t.ToString()));
        }

        class FakeAstNode : AstNode
        {
            public FakeAstNode() : base(0, 0)
            {
            }
        }
    }
}
