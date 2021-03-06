using CParser.Translation;
using CParser.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CParser.Lexing.Terminal;
using static CParser.Lexing.LexerState;
using static CParser.Translation.SymbolType;
using System.Threading.Tasks.Dataflow;

namespace CParser.Lexing
{
    public class Lexer : IAsyncStream<Token>
    {
        public TranslationUnit TranslationUnit { get; }
        protected AsyncStreamWrapper<Token> OutputStream { get; }
        protected IAsyncStream<char> InputStream { get; }
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

        public Lexer(TranslationUnit tu, IAsyncStream<char> input, bool preprocessorTokens, bool outputTrivia)
        {
            TranslationUnit = tu;
            InputStream = input;
            PreprocessorTokens = preprocessorTokens;
            OutputTrivia = outputTrivia;
            Filename = tu.CurrentFilename;
            OutputStream = new AsyncStreamWrapper<Token>(Lex(), Token.Eof);
        }

        public static AsyncStreamFunc<char, Token> Streaming(TranslationUnit translationUnit, bool preprocessorTokens, bool outputTrivia)
        {
            return charStream => new Lexer(translationUnit, charStream, preprocessorTokens, outputTrivia);
        }

        public static IPropagatorBlock<char, Token> AsBlock(TranslationUnit translationUnit, bool preprocessorTokens, bool outputTrivia)
        {
            return Streaming(translationUnit, preprocessorTokens, outputTrivia).Buffered();
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
                        sb.Append(await InputStream.Read());
                    }
                    TranslationUnit.LexerState = LexerState.LexerReady;
                    yield return new ValueToken<string>(Terminal.Filename, line, column, filename, sb.ToString());
                    continue; // Parse the next token
                }
                c = await InputStream.Read();
                if (char.IsLetter(c) || c == '_')
                {
                    var sb = new StringBuilder(c.ToString());
                    Symbol symbol;
                    while ((c = await InputStream.Peek()) == '_' || char.IsLetter(c) || char.IsDigit(c))
                    {
                        sb.Append(await InputStream.Read());
                    }
                    var ident = sb.ToString();
                    if (ReservedWords.ContainsKey(ident))
                    {
                        yield return new Token(ReservedWords[ident], line, column, filename);
                    }
                    else if (TranslationUnit.Symbols.ContainsKey(ident)
                        && ((symbol = TranslationUnit.Symbols[ident]).Type == SymbolType.EnumSymbol
                            || symbol.Type == TypedefSymbol))
                    {
                        yield return symbol.Type == TypedefSymbol 
                            ? new ValueToken<Symbol>(TypedefName, line, column, filename, symbol) as Token
                            : new ValueToken<EnumSymbol>(EnumConstant, line, column, filename, (symbol as EnumSymbol)!) as Token;
                    }
                    else
                    {
                        yield return new ValueToken<string>(Identifier, line, column, filename, ident);
                    }
                }
                else if (char.IsDigit(c))
                {
                    var integer = 0UL + c - '0';
                    var fraction = 0M;
                    var real = 0M;
                    var hexOrOctal = false;
                    var floating = false;
                    var unsigned = false;
                    var isLong = false;
                    var nonDouble = false;
                    var d = 0d;
                    if (c == '0')
                    {
                        if ((c = await InputStream.Peek()) == 'x' || c == 'X')
                        {
                            await InputStream.Read();
                            while (char.IsDigit(c = char.ToLower(await InputStream.Peek())) || (c >= 'a' && c <= 'f'))
                            {
                                await InputStream.Read();
                                integer *= 0x10;
                                integer += (ulong)(char.IsDigit(c) ? c - '0' : c - 'a' + 0xa);
                            }
                            hexOrOctal = true;
                        }
                        else if ((c = await InputStream.Peek()) >= '0' && c <= '7') // Octal
                        {
                            while ((c = await InputStream.Peek()) >= '0' && c <= '7')
                            {
                                c = await InputStream.Read();
                                integer *= 8;
                                integer += (ulong)(c - '0');
                            }
                            hexOrOctal = true;
                        }
                    }
                    if (!hexOrOctal)
                    {
                        while (char.IsDigit(await InputStream.Peek()))
                        {
                            c = await InputStream.Read();
                            integer *= 10;
                            integer += (ulong)(c - '0');
                        }
                        if (await InputStream.Peek() == '.')
                        {
                            var multiplier = 0.1M;
                            floating = true;
                            await InputStream.Read();
                            while (char.IsDigit(await InputStream.Peek()))
                            {
                                c = await InputStream.Read();
                                fraction += (c - '0') * multiplier;
                                multiplier *= 0.1M;
                            }
                            d = (double)(integer + fraction);
                        }
                        if ((c = await InputStream.Peek()) == 'e' || c == 'E')
                        {
                            floating = true;
                            real = integer + fraction;
                            var eSign = 1;
                            var e = 0;
                            await InputStream.Read();
                            if ((c = await InputStream.Peek()) == '+' || c == '-')
                            {
                                await InputStream.Read();
                                eSign = c == '-' ? -1 : 1;
                            }
                            while (char.IsDigit(await InputStream.Peek()))
                            {
                                c = await InputStream.Read();
                                e *= 10;
                                e += c - '0';
                            }
                            if (real != 0m)
                            {
                                e *= eSign;
                                d = (double)real * Math.Pow(10.0, (double)e);
                                while (real >= 10m)
                                {
                                    real /= 10m;
                                    e += eSign;
                                }
                                while (real < 1m)
                                {
                                    real *= 10m;
                                    e -= eSign;
                                }
                                if (e == 308 && real >= 1.7976931348623157m || e > 308)
                                {
                                    d = double.MaxValue;
                                }
                                else if (e == -324 && real <= 4.94065645841247m || e < -324)
                                {
                                    d = double.Epsilon;
                                }
                            }
                        }
                    }
                    while ((c = await InputStream.Peek()) == 'l' || c == 'L' || c == 'u' || c == 'U' || c == 'f' || c == 'F')
                    {
                        switch (c = char.ToLower(await InputStream.Read()))
                        {
                            case 'l':
                                isLong = true;
                                break;
                            case 'u':
                                unsigned = true;
                                break;
                            case 'f':
                                if (!floating)
                                {
                                    real = integer;
                                    floating = true;
                                }
                                nonDouble = true;
                                break;
                        }
                    }
                    var valid = true;
                    if (floating && (isLong || unsigned))
                    {
                        Error("Invalid suffix combination");
                        valid = false;
                    }
                    if (floating)
                    {
                        var token = new FloatingToken(line, column, filename, d, nonDouble);
                        token.IsValid = valid;
                        yield return token;
                    }
                    else
                    {
                        yield return new IntegerToken(line, column, filename, integer, unsigned, isLong);
                    }
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
                            while (!await InputStream.Eof() && (c = await InputStream.Peek()) != delimeter && c != '\r' && c != '\n')
                            {
                                if (c == '\\')
                                {
                                    await InputStream.Read();
                                    if (!await InputStream.Eof())
                                    {
                                        c = await InputStream.Read();
                                        if (char.IsDigit(c) && c < '8')
                                        {
                                            int cc = c - '0';
                                            for (var i = 0; i < 2 && char.IsDigit(c = await InputStream.Peek()) && c < '8'; i++)
                                            {
                                                cc *= 8;
                                                cc += await InputStream.Read() - '0';
                                            }
                                            sb.Append((char)cc);
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
                                                case 'X':
                                                {
                                                    var letter = c;
                                                    int i;
                                                    int cc = 0;
                                                    for (i = 0; i < 2 && (char.IsDigit(c = char.ToLower(await InputStream.Peek())) || (c >= 'a' && c <= 'f')); i++)
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
                                                        sb.Append("\\"+letter);
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
                            var valid = true;
                            if (await InputStream.Eof() || (c = await InputStream.Peek()) != delimeter)
                            {
                                Error($"{delimeter} expected");
                                valid = false;
                            }
                            else
                            {
                                await InputStream.Read();
                            }
                            if (delimeter == '\'' && sb.Length != 1)
                            {
                                Error("Invalid character literal");
                                valid = false;
                            }
                            var token = new ValueToken<string>(delimeter == '\"' ? StringLiteral : CharLiteral, line, column, filename, sb.ToString());
                            token.IsValid = valid;
                            yield return token;
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
                            else
                            {
                                if (!PreprocessorTokens)
                                {
                                    Error("Unexpected symbol #");
                                }

                                yield return PreprocessorTokens
                                    ? new Token(Pound, line, column, filename)
                                    : new ValueToken<char>(Terminal.Unknown, line, column, filename, '#');
                            }
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
                                    yield return new ValueToken<string>(Comment, line, column, filename, sb!.ToString().Trim());
                                }
                            }
                            else if (await InputStream.Peek() == '*')
                            {
                                await InputStream.Read();
                                var sb = OutputTrivia ? new StringBuilder() : null;
                                var terminated = false;
                                while (!await InputStream.Eof())
                                {
                                    if (await InputStream.Peek() == '*')
                                    {
                                        await InputStream.Read();
                                        if (await InputStream.Peek() == '/')
                                        {
                                            await InputStream.Read();
                                            terminated = true;
                                            break;
                                        }
                                        else if (OutputTrivia)
                                        {
                                            sb!.Append('*');
                                        }
                                    }
                                    else
                                    {
                                        c = await InputStream.Read();
                                        if (OutputTrivia)
                                        {
                                            sb!.Append(c);
                                        }
                                    } 
                                }
                                if (!terminated)
                                {
                                    Error("Unterminated comment.");
                                }
                                if (OutputTrivia)
                                {
                                    var token = new ValueToken<string>(Comment, line, column, filename, sb!.ToString().Trim());
                                    if (!terminated)
                                    {
                                        token.IsValid = false;
                                    }
                                    yield return token;
                                }
                            }
                            else
                            {
                                yield return new Token(Slash, line, column, filename);
                            }
                            break;
                        default:
                            yield return new ValueToken<char>(Terminal.Unknown, line, column, filename, c);
                            Error($"Unexpected symbol {c}");
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