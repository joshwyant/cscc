using cscc.Translation;
using cscc.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static cscc.Lexing.Terminal;
using static cscc.Lexing.LexerState;

namespace cscc.Lexing
{
    class Lexer : IStream<Token>
    {
        public TranslationUnit TranslationUnit { get; }
        protected StreamWrapper<Token> OutputStream { get; }
        protected IStream<char> InputStream { get; }
        public Token Sentinel => 
            new Token(
                Terminal.Eof, 
                TranslationUnit.CurrentLine, 
                TranslationUnit.CurrentColumn, 
                TranslationUnit.CurrentFilename);
        public bool OutputTrivia { get; }
        public bool PreprocessorTokens { get; }
        public string Filename { get; set; }
        public static Dictionary<string, Terminal> ReservedWords 
            = new Dictionary<string, Terminal>
        {
            { "auto", Auto },
            { "asm", Asm },
            { "break", Break },
            { "case", Case },
            { "char", Terminal.Char },
            { "const", Const },
            { "continue", Continue },
            { "default", Default },
            { "do", Do },
            { "double", Terminal.Double },
            { "else", Else },
            { "enum", Terminal.Enum },
            { "extern", Extern },
            { "float", Float },
            { "for", For },
            { "goto", Goto },
            { "if", If },
            { "int", Int },
            { "long", Long },
            { "register", Register },
            { "return", Return },
            { "short", Short },
            { "signed", Signed },
            { "sizeof", Sizeof },
            { "static", Static },
            { "struct", Struct },
            { "switch", Switch },
            { "typedef", Typedef },
            { "union", Union },
            { "unsigned", Unsigned },
            { "void", Terminal.Void },
            { "volatile", Terminal.Volatile },
            { "while", While },
        };

        public Lexer(TranslationUnit tu, IStream<char> input, bool preprocessorTokens, bool outputTrivia)
        {
            TranslationUnit = tu;
            InputStream = input;
            PreprocessorTokens = preprocessorTokens;
            OutputTrivia = outputTrivia;
            Filename = tu.CurrentFilename;
            OutputStream = new StreamWrapper<Token>(Lex());
        }

        protected async IAsyncEnumerable<Token> Lex()
        {
            char c;
            while (!await InputStream.Eof())
            {
                int line = TranslationUnit.CurrentLine, column = TranslationUnit.CurrentColumn;
                string filename = TranslationUnit.CurrentFilename;
                // #include <FILENAME_TO_PARSE>
                if (TranslationUnit.LexerState == LexingLibraryFilename)
                {
                    var sb = new StringBuilder();
                    while (!await InputStream.Eof() && ((c = await InputStream.Peek()) != '>'))
                    {
                        sb.Append(c);
                    }
                    TranslationUnit.LexerState = LexerState.LexerReady;
                    yield return new ValueToken<string>(Terminal.Filename, line, column, filename, sb.ToString());
                    continue; // Parse the next token
                }
                c = await InputStream.Read();
                if (char.IsLetter(c) || c == '_')
                {
                    // TODO: Identifiers and built-ins
                    // Include: Identifier, TypedefName, and EnumConstant
                }
                else if (char.IsDigit(c))
                {
                    // TODO: Numbers, hex, and octal
                    // IntegerConstant and FloatingConstant
                }
                else
                {
                    switch (c)
                    {
                        case ' ':
                        case '\t':
                            while (!await InputStream.Eof() && ((c = await InputStream.Peek()) == ' ' || c == '\t'))
                            {
                                await InputStream.Read();
                            }
                            if (OutputTrivia)
                            {
                                yield return new Token(Whitespace, line, column, filename);
                            }
                            break;
                        case '\r':
                            if (await InputStream.Peek() == '\n')
                            {
                                await InputStream.Read();
                            }
                            if (OutputTrivia)
                            {
                                yield return new Token(Newline, line, column, filename);
                            }
                            break;
                        case '\n':
                            if (OutputTrivia)
                            {
                                yield return new Token(Newline, line, column, filename);
                            }
                            break;
                        case '\"':
                        case '\'':
                        {
                            char delimeter = c;
                            var sb = new StringBuilder();
                            while (!await InputStream.Eof() && (c = await InputStream.Peek()) != delimeter && c != '\r' && c == '\n')
                            {
                                if (c == '\\')
                                {
                                    if (!await InputStream.Eof())
                                    {
                                        c = await InputStream.Read();
                                        if (char.IsDigit(c))
                                        {
                                            int i;
                                            int cc = 0;
                                            for (i = 0; i < 3 && (char.IsDigit(c = await InputStream.Peek()) || char.ToLower(c) >= 'a' || char.ToLower(c) <= 'f'); i++)
                                            {
                                                await InputStream.Read();
                                                cc *= 8;
                                                cc += c - '0';
                                            }
                                            sb.Append((char)cc);
                                            break;
                                        }
                                        else
                                        {
                                            switch (c)
                                            {
                                                case 'n':
                                                    sb.Append('\n');
                                                    break;
                                                case 't':
                                                    sb.Append('\t');
                                                    break;
                                                case 'v':
                                                    sb.Append('\v');
                                                    break;
                                                case 'b':
                                                    sb.Append('\b');
                                                    break;
                                                case 'r':
                                                    sb.Append('\r');
                                                    break;
                                                case 'f':
                                                    sb.Append('\f');
                                                    break;
                                                case 'a':
                                                    sb.Append('\a');
                                                    break;
                                                case '\\':
                                                    sb.Append('\\');
                                                    break;
                                                case '?':
                                                    sb.Append('?');
                                                    break;
                                                case '\'':
                                                    sb.Append('\'');
                                                    break;
                                                case '"':
                                                    sb.Append('"');
                                                    break;
                                                case 'x':
                                                {
                                                    await InputStream.Read();
                                                    int i;
                                                    int cc = 0;
                                                    for (i = 0; i < 2 && (char.IsDigit(c = await InputStream.Peek()) || char.ToLower(c) >= 'a' || char.ToLower(c) <= 'f'); i++)
                                                    {
                                                        await InputStream.Read();
                                                        cc *= 0x10;
                                                        if (char.IsDigit(c))
                                                        {
                                                            cc += c - '0';
                                                        }
                                                        else
                                                        {
                                                            cc += char.ToLower(c) - 'a' + 0xa;
                                                        }
                                                    }
                                                    if (i == 0)
                                                    {
                                                        sb.Append("\\x");
                                                    }
                                                    else
                                                    {
                                                        sb.Append((char)cc);
                                                    }
                                                    break;
                                                }
                                                default:
                                                    sb.Append('\\');
                                                    sb.Append(c);
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    sb.Append(await InputStream.Read());
                                }
                            }
                            if (await InputStream.Eof() || (c = await InputStream.Peek()) != delimeter)
                            {
                                Error($"{delimeter} expected");
                            }
                            else
                            {
                                await InputStream.Read();
                            }
                            yield return new ValueToken<string>(Terminal.StringLiteral, line, column, filename, sb.ToString());
                            break;
                        }
                        case '~':
                            yield return new Token(Tilde, line, column, filename);
                            break;
                        case '#':
                            if (PreprocessorTokens && await InputStream.Peek() == '#')
                            {
                                await InputStream.Read();
                                yield return new Token(DoublePound, line, column, filename);
                            }
                            yield return PreprocessorTokens 
                                ? new Token(Pound, line, column, filename)
                                : new ValueToken<char>(Unknown, line, column, filename, '#');
                            break;
                        case '(':
                            yield return new Token(LeftParen, line, column, filename);
                            break;
                        case ')':
                            yield return new Token(RightParen, line, column, filename);
                            break;
                        case '{':
                            yield return new Token(LeftBrace, line, column, filename);
                            break;
                        case '}':
                            yield return new Token(RightBrace, line, column, filename);
                            break;
                        case '[':
                            yield return new Token(LeftBracket, line, column, filename);
                            break;
                        case ']':
                            yield return new Token(RightBracket, line, column, filename);
                            break;
                        case ':':
                            yield return new Token(Colon, line, column, filename);
                            break;
                        case ';':
                            yield return new Token(Semicolon, line, column, filename);
                            break;
                        case ',':
                            yield return new Token(Comma, line, column, filename);
                            break;
                        case '.':
                            yield return new Token(Dot, line, column, filename);
                            break;
                        case '?':
                            yield return new Token(Query, line, column, filename);
                            break;
                        case '!':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(NotEqual, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Bang, line, column, filename);
                            }
                            break;
                        case '%':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(ModAssign, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Percent, line, column, filename);
                            }
                            break;
                        case '^':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(XorAssign, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Caret, line, column, filename);
                            }
                            break;
                        case '&':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(AndAssign, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '&')
                            {
                                await InputStream.Read();
                                yield return new Token(LogicalAnd, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Ampersand, line, column, filename);
                            }
                            break;
                        case '*':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(MultiplyAssign, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Star, line, column, filename);
                            }
                            break;
                        case '-':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(SubtractAssign, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '-')
                            {
                                await InputStream.Read();
                                yield return new Token(Decrement, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '>')
                            {
                                await InputStream.Read();
                                yield return new Token(Arrow, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Minus, line, column, filename);
                            }
                            break;
                        case '+':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(AddAssign, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '+')
                            {
                                await InputStream.Read();
                                yield return new Token(Increment, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Plus, line, column, filename);
                            }
                            break;
                        case '=':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(DoubleEquals, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Assign, line, column, filename);
                            }
                            break;
                        case '|':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(OrAssign, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '|')
                            {
                                await InputStream.Read();
                                yield return new Token(LogicalOr, line, column, filename);
                            }
                            else
                            {
                                yield return new Token(Pipe, line, column, filename);
                            }
                            break;
                        case '<':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(LessThanOrEqual, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '<')
                            {
                                await InputStream.Read();
                                if (await InputStream.Peek() == '=')
                                {
                                    await InputStream.Read();
                                    yield return new Token(ShiftLeftAssign, line, column, filename);
                                }
                                else
                                {
                                    yield return new Token(ShiftLeft, line, column, filename);
                                }
                            }
                            else
                            {
                                yield return new Token(LessThan, line, column, filename);
                            }
                            break;
                        case '>':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(GreaterThanOrEqual, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '>')
                            {
                                await InputStream.Read();
                                if (await InputStream.Peek() == '=')
                                {
                                    await InputStream.Read();
                                    yield return new Token(ShiftRightAssign, line, column, filename);
                                }
                                else
                                {
                                    yield return new Token(ShiftRight, line, column, filename);
                                }
                            }
                            else
                            {
                                yield return new Token(GreaterThan, line, column, filename);
                            }
                            break;
                        case '/':
                            if (await InputStream.Peek() == '=')
                            {
                                await InputStream.Read();
                                yield return new Token(DivideAssign, line, column, filename);
                            }
                            else if (await InputStream.Peek() == '/')
                            {
                                await InputStream.Read();
                                var sb = OutputTrivia ? new StringBuilder() : null;
                                while (!await InputStream.Eof() && await InputStream.Peek() != '\r' && await InputStream.Peek() != '\n')
                                {
                                    c = await InputStream.Read();
                                    if (OutputTrivia)
                                    {
                                        sb!.Append(c);
                                    }
                                }
                                if (OutputTrivia)
                                {
                                    yield return new ValueToken<string>(Comment, line, column, filename, sb!.ToString());
                                }
                            }
                            else if (await InputStream.Peek() == '*')
                            {
                                await InputStream.Read();
                                var sb = OutputTrivia ? new StringBuilder() : null;
                                while (!await InputStream.Eof())
                                {
                                    if (await InputStream.Peek() == '*')
                                    {
                                        await InputStream.Read();
                                        if (await InputStream.Peek() == '/')
                                        {
                                            await InputStream.Read();
                                            break;
                                        }
                                        else if (OutputTrivia)
                                        {
                                            sb!.Append('*');
                                        }
                                    }
                                    else if (OutputTrivia)
                                    {
                                        sb!.Append(await InputStream.Read());
                                    }
                                }
                                if (OutputTrivia)
                                {
                                    yield return new ValueToken<string>(Comment, line, column, filename, sb!.ToString());
                                }
                            }
                            else
                            {
                                yield return new Token(Slash, line, column, filename);
                            }
                            break;
                        default:
                            yield return new ValueToken<char>(Unknown, line, column, filename, c);
                            break;
                    }
                }
            }
            yield return Sentinel;
        }

        protected void Error(string message)
        {
            TranslationUnit.Errors.Add(new CompileError(TranslationUnit.CurrentLine, TranslationUnit.CurrentColumn, message));
        }

        #region IAsyncEnumerable<Token> members
        public IAsyncEnumerator<Token> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return OutputStream.GetAsyncEnumerator();
        }
        #endregion

        #region IStream<Token> members
        public async Task<bool> Eof()
        {
            return await OutputStream.Eof();
        }

        public async Task<Token> Peek()
        {
            return await OutputStream.Peek();
        }

        public async Task<Token> Read()
        {
            return await OutputStream.Read();
        }

        public void PutBack(Token val)
        {
            OutputStream.PutBack(val);
        }
        #endregion
    }
}