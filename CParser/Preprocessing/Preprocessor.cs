using CParser.Translation;
using CParser.Helpers;
using CParser.Lexing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CParser.Lexing.Terminal;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using static CParser.Helpers.Functions;

namespace CParser.Preprocessing
{
    public class Preprocessor : IAsyncStream<Token>
    {
        private const int TAB_WIDTH = 8;
        public TranslationUnit TranslationUnit { get; }
        protected TextReader Reader { get; }
        protected IAsyncStream<Token> OutputStream { get; }
        public Token Sentinel =>
            new Token(
                Terminal.Eof,
                TranslationUnit.CurrentLine,
                TranslationUnit.CurrentColumn,
                TranslationUnit.CurrentFilename);
        protected HashSet<string> MacroStack { get; }

        public Preprocessor(TranslationUnit tu, TextReader reader, bool preprocess)
        {
            MacroStack = new HashSet<string>();
            Reader = reader;
            TranslationUnit = tu;
            OutputStream = preprocess
                ? Preprocess()
                : TokenizeOnly();
        }

        protected IAsyncStream<Token> ProcessPipeline(TextReader reader, AsyncStreamFunc<char, Token> pipeline)
        {
            return pipeline(reader.ToStream()).ToStream(Sentinel)!;
        }

        protected IAsyncStream<Token> ProcessPipeline(string filename, AsyncStreamFunc<char, Token> pipeline)
        {
            return ProcessPipeline(
                    TranslationUnit
                        .FileResolver
                        .ResolveTextReader(filename),
                    pipeline);
        }

        protected AsyncStreamFunc<char, Token> BasicPipeline(bool preprocessing)
        {
            return Compose<char>(CountLines, 
                                 ReplaceTrigraphs, 
                                 SpliceLines)
                   .Chain(charStream => 
                                       new Lexer(TranslationUnit, 
                                                 charStream,
                                                 preprocessing, preprocessing));
        }

        protected AsyncStreamFunc<char, Token> IncludePipeline()
        {
            return BasicPipeline(true) // preprocess
                    .Chain(stream =>
                                        RemoveTrivia(stream!, true), Sentinel)! // keep newlines
                    .Chain(IncludeFiles!)!
                    .Chain(AssembleBuffers!);
        }

        protected AsyncStreamFunc<char, Token> FullPipeline()
        {
            return IncludePipeline()!
                    .Chain(DoPreprocess!)
                    .Chain(stream => RemoveTrivia(stream!), Sentinel); // remove all the newlines
        }

        protected async IAsyncEnumerable<char> CountLines(IAsyncStream<char> input)
        {
            await foreach(char c in input)
            {
                yield return c;
                if (c == '\r')
                {
                    if (!await input.Eof() && await input.Peek() == '\n')
                    {
                        yield return await input.Read();
                    }
                    TranslationUnit.CurrentLine++;
                    TranslationUnit.CurrentColumn = 1;
                }
                else if (c == '\n')
                {
                    TranslationUnit.CurrentLine++;
                    TranslationUnit.CurrentColumn = 1;
                }
                else if (c == '\t')
                {
                    // TODO: Is this the expectation for tabs?
                    TranslationUnit.CurrentColumn += TAB_WIDTH - ((TranslationUnit.CurrentColumn - 1) % TAB_WIDTH);
                }
                else
                {
                    TranslationUnit.CurrentColumn++;
                }
            }
        }

        protected async IAsyncEnumerable<char> ReplaceTrigraphs(IAsyncStream<char> input)
        {
            int num_question_marks = 0;
            char c;
            while (!await input.Eof())
            {
                switch (c = await input.Read())
                {
                    case '?':
                        num_question_marks++;
                        break;
                    default:
                        if (num_question_marks >= 2)
                        {
                            while (num_question_marks > 2)
                            {
                                yield return '?';
                                num_question_marks--;
                            }
                            switch (c)
                            {
                                case '=':
                                    yield return '#';
                                    break;
                                case '/':
                                    yield return '\\';
                                    break;
                                case '\'':
                                    yield return '^';
                                    break;
                                case '(':
                                    yield return '[';
                                    break;
                                case ')':
                                    yield return ']';
                                    break;
                                case '!':
                                    yield return '|';
                                    break;
                                case '<':
                                    yield return '{';
                                    break;
                                case '>':
                                    yield return '}';
                                    break;
                                case '-':
                                    yield return '~';
                                    break;
                                default:
                                    while (num_question_marks > 0)
                                    {
                                        yield return '?';
                                    }
                                    yield return c;
                                    break;
                            }
                            num_question_marks = 0;
                        }
                        else
                        {
                            yield return c;
                        }
                        break;
                }
            }
            while (num_question_marks > 0)
            {
                yield return '?';
            }
        }

        protected async IAsyncEnumerable<char> SpliceLines(IAsyncStream<char> input)
        {
            char c;
            while (!await input.Eof())
            {
                switch (c = await input.Read())
                {
                    // Splice lines ending in '\' with the next line.
                    case '\\':
                        bool whitespace = false;
                        while (!await input.Eof() && (await input.Peek() == ' ' || await input.Peek() == '\t'))
                        {
                            whitespace = true;
                            await input.Read();
                        }
                        if (await input.Eof())
                        {
                            if (whitespace)
                            {
                                yield return ' ';
                            }
                            yield return '\\';
                        }
                        else
                        {
                            c = await input.Read();
                            if (c == '\r')
                            {
                                if (!await input.Eof() && await input.Peek() == '\n')
                                {
                                    await input.Read();
                                }
                            }
                            else if (c != '\n')
                            {
                                yield return '\\';
                                if (whitespace)
                                {
                                    yield return ' ';
                                }
                                yield return c;
                            }
                        }
                        break;
                    case '\r': // Convert "\r\n" to "\n"
                        if (!await input.Eof() && await input.Peek() == '\n')
                        {
                            await input.Read();
                        }
                        yield return '\n';
                        break;
                    default:
                        yield return c;
                        break;
                }
            }
        }

        protected async Task ProcessIncludeFileLine(IAsyncStream<Token> input, BufferBlock<Token> tokenBuffer)
        {
            Terminal? t = default;
            if ((await input.Peek()).Kind == Pound)
            {
                var poundToken = (await input.Read());
                if ((await input.Peek()).Kind == Identifier)
                {
                    ValueToken<string> directiveToken;
                    switch ((directiveToken = (ValueToken<string>)(await input.Read())).Value.ToLower())
                    {
                        case "include":
                            if ((t = (await input.Peek()).Kind) == LessThan || t == StringLiteral)
                            {
                                string? filename = null;
                                if (t == StringLiteral)
                                {
                                    var token = await input.Read();
                                    filename = ((ValueToken<string>)token).Value;
                                    if (!await input.Eof())
                                    {
                                        if ((await input.Peek()).Kind != Newline)
                                        {
                                            // TODO: Error
                                        }
                                        else
                                        {
                                            tokenBuffer.Post(await input.Read());
                                        }
                                        // TODO: Error
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        var token = await input.Read();
                                        TranslationUnit.LexerState = LexerState.LexingLibraryFilename;
                                        if ((await input.Peek()).Kind == Filename)
                                        {
                                            filename = ((ValueToken<string>)await input.Read()).Value;
                                        }
                                    }
                                    finally
                                    {
                                        TranslationUnit.LexerState = LexerState.LexerReady;
                                    }
                                    if ((await input.Peek()).Kind != GreaterThan)
                                    {
                                        // TODO: Error
                                    }
                                    else
                                    {
                                        await input.Read();
                                    }
                                    if (!await input.Eof())
                                    {
                                        if ((await input.Peek()).Kind != Newline)
                                        {
                                            // TODO: Error
                                        }
                                        else
                                        {
                                            tokenBuffer.Post(await input.Read());
                                        }
                                        // TODO: Error
                                    }
                                }
                                if (filename != null)
                                {
                                    var dummyTask = tokenBuffer.PostAllAsync(
                                            ProcessPipeline(filename, IncludePipeline()!
                                                .Chain(FilterEof!)))
                                        .ContinueWith(_ => tokenBuffer.Complete());
                                }
                            }
                            break;
                        default:
                        {
                            // Pass directive on to the rest of the pipeline
                            // (this method only handles the #include directive)
                            tokenBuffer.Post(poundToken);
                            tokenBuffer.Post(directiveToken);
                            while (!await input.Eof())
                            {
                                var token = await input.Read();
                                tokenBuffer.Post(token);
                                if (token.Kind == Newline) break;
                            }
                            tokenBuffer.Complete();
                            break;
                        }
                    }
                }
                else
                {
                    // Pass line on to the rest of the pipeline
                    tokenBuffer.Post(poundToken);
                    while (!await input.Eof())
                    {
                        var token = await input.Read();
                        tokenBuffer.Post(token);
                        if (token.Kind == Newline) break;
                    }
                    tokenBuffer.Complete();
                }
            }
            else
            {
                while (!await input.Eof())
                {
                    var token = await input.Read();
                    tokenBuffer.Post(token);
                    if (token.Kind == Newline) break;
                }
                tokenBuffer.Complete();
            }
        }

        protected async IAsyncEnumerable<BufferBlock<Token>> IncludeFiles(IAsyncStream<Token> input)
        {
            while (!await input.Eof())
            {
                // Preprocessor posts processed tokens onto this buffer block.
                var tokenBuffer = new BufferBlock<Token>();
                var task = ProcessIncludeFileLine(input, tokenBuffer);
                yield return tokenBuffer;
                await task;
            }
        }

        protected async IAsyncEnumerable<Token> AssembleBuffers(IAsyncStream<BufferBlock<Token>> lines)
        {
            var lineBuffer = new BufferBlock<BufferBlock<Token>>();
            var task = lineBuffer.PostAllAsync(lines)
                .ContinueWith(_ => lineBuffer.Complete());

            await foreach (var line in lineBuffer.ReceiveAllAsync())
            {
                await foreach (var token in line.ReceiveAllAsync())
                {
                    yield return token;
                }
            }
        }

        protected IAsyncStream<Token> Preprocess()
        {
            return ProcessPipeline(Reader, FullPipeline());
        }

        protected IAsyncStream<Token> TokenizeOnly()
        {
            return ProcessPipeline(Reader, BasicPipeline(false));
        }

        protected async IAsyncEnumerable<Token> DoPreprocess(IAsyncStream<Token> input)
        {
            while (!await input.Eof())
            {
                Token token;
                if ((await input.Peek()).Kind == Pound)
                {
                    await input.Read();
                    if ((await input.Peek()).Kind == Identifier)
                    {
                        switch ((await input.Read() as ValueToken<string>)!.Value.ToLower())
                        {
                            case "define":
                            {
                                if ((await input.Peek()).Kind == Identifier)
                                {
                                    var ident = (ValueToken<string>)await input.Read();
                                    var list = new List<Token>();
                                    while (!await input.Eof())
                                    {
                                        list.Add(token = await input.Read());
                                        if (token.Kind == Newline) break;
                                    }
                                    var define = new DefineSymbol(SymbolType.DefineSymbol,
                                        ident!.Value, list);
                                    TranslationUnit.Defines.Add(ident.Value, define);
                                }
                                break;
                            }
                            case "undef":
                                if ((await input.Peek()).Kind == Identifier)
                                {
                                    var ident = (ValueToken<string>)await input.Read();
                                    TranslationUnit.Defines.Remove(ident.Value);
                                }
                                break;
                            case "line":
                                // TODO: The rest of the preprocessor directives
                                break;
                            case "error":
                                break;
                            case "pragma":
                                break;
                            case "if":
                                break;
                            case "ifdef":
                                break;
                            case "ifndef":
                                break;
                        }
                    }
                }
                while (!await input.Eof())
                {
                    token = await input.Read();
                    if (token.Kind == Identifier)
                    {
                        foreach (var t in MaybeSubstitute((ValueToken<string>)token))
                        {
                            yield return t;
                        }
                    }
                    else
                    {
                        yield return token;
                    }
                    if (token.Kind == Newline) break;
                }
            }
        }

        protected IPropagatorBlock<IEnumerable<Token>, Token> FlattenLines(ISourceBlock<IEnumerable<Token>> source)
        {
            return new TransformManyBlock<IEnumerable<Token>, Token>(tokens => tokens);
        }

        protected async IAsyncEnumerable<Token> RemoveTrivia(IAsyncStream<Token> input, bool preserveNewlines = false)
        {
            await foreach (var token in input)
            {
                if (!preserveNewlines && token.Kind == Newline)
                {
                    continue;
                }
                else if (token.Kind != Whitespace && token.Kind != Comment)
                {
                    yield return token;
                }
            }
        }

        protected async IAsyncEnumerable<Token> ScanLine(IAsyncStream<Token> input)
        {
            await foreach (var token in input)
            {
                yield return token;
                if (token.Kind == Newline)
                {
                    yield break;
                }
            }
        }

        protected async IAsyncEnumerable<Token> FilterEof(IAsyncStream<Token> input)
        {
            await foreach (var token in input)
            {
                if (token.Kind != Terminal.Eof)
                {
                    yield return token;
                }
            }
        }

        IEnumerable<Token> MaybeSubstitute(ValueToken<string> t)
        {
            if (TranslationUnit.Defines.ContainsKey(t.Value))
            {
                foreach (var define_token in Substitute(t))
                {
                    yield return define_token;
                }
            }
            else
            {
                yield return t;
            }
        }

        IEnumerable<Token> Substitute(ValueToken<string> t)
        {
            var s = TranslationUnit.Defines[t.Value];
            if (MacroStack.Contains(s.Name))
            {
                // Can't recursively call macros.
                yield return t;
                yield break;
            }
            MacroStack.Add(s.Name);
            try
            {
                switch (s)
                {
                    case DefineSymbol d:
                        foreach (var define_token in d.Definition)
                        {
                            switch (define_token)
                            {
                                case ValueToken<string> string_token when string_token.Kind == Identifier:
                                    foreach (var substitution_token in MaybeSubstitute(string_token))
                                    {
                                        yield return substitution_token.Copy(
                                            TranslationUnit.CurrentLine,
                                            TranslationUnit.CurrentColumn,
                                            TranslationUnit.CurrentFilename);
                                    }
                                    break;
                                // TODO: trigraphs, etc.
                                default:
                                    yield return define_token;
                                    break;
                            }
                        }
                        break;
                    case MacroSymbol m:
                        break;
                }
            }
            finally
            {
                MacroStack.Remove(s.Name);
            }
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