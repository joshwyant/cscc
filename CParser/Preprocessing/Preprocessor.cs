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

        protected async Task ProcessPipeline(TextReader reader, IPropagatorBlock<char, Token> pipeline)
        {
            await pipeline.PostAllTextAsync(reader);
            pipeline.Complete();
        }

        protected Task ProcessPipeline(string filename, IPropagatorBlock<char, Token> pipeline)
        {
            return ProcessPipeline(
                    TranslationUnit
                        .FileResolver
                        .ResolveTextReader(filename),
                    pipeline);
        }

        protected IPropagatorBlock<char, Token> BasicPipeline(bool preprocessing)
        {
            return Compose<char>(CountLines, 
                                 ReplaceTrigraphs, 
                                 SpliceLines)
                   .StreamAndChain(charStream => 
                                       new Lexer(TranslationUnit, 
                                                 charStream,
                                                 preprocessing, preprocessing));
        }

        protected IPropagatorBlock<char, Token> IncludePipeline()
        {
            return BasicPipeline(true) // preprocess
                    .StreamAndChain(stream =>
                                        RemoveTrivia(stream!, true)) // keep newlines
                    .StreamAndChain(CollectLines!)
                    .StreamAndChain(IncludeFiles!)
                    .StreamAndChain(AssembleBuffers!);
        }

        protected IPropagatorBlock<char, Token> FullPipeline()
        {
            return IncludePipeline()
                    .StreamAndChain(CollectLines!)
                    .StreamAndChain(DoPreprocess!)
                    .StreamAndChain(stream => RemoveTrivia(stream!)); // remove all the newlines
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

        protected async IAsyncEnumerable<BufferBlock<Token>> IncludeFiles(IAsyncStream<IStream<Token>> lines)
        {
            Terminal? t = default;
            await foreach (var line in lines)
            {
                // Preprocessor posts processed tokens onto this buffer block.
                var tokenBuffer = new BufferBlock<Token>();
                yield return tokenBuffer;
                if (line.Peek().Kind == Pound)
                {
                    line.Read();
                    if (line.Peek().Kind == Identifier)
                    {
                        switch ((line.Read() as ValueToken<string>)!.Value.ToLower())
                        {
                            case "include":
                                try
                                {
                                    if ((t = line.Peek().Kind) == LessThan || t == StringLiteral)
                                    {
                                        var token = line.Read();
                                        string? filename = null;
                                        if (t == StringLiteral)
                                        {
                                            filename = ((ValueToken<string>)token).Value;
                                        }
                                        else
                                        {
                                            TranslationUnit.LexerState = LexerState.LexingLibraryFilename;
                                            if (line.Peek().Kind == Filename)
                                            {
                                                filename = ((ValueToken<string>)line.Read()).Value;
                                            }
                                            TranslationUnit.LexerState = LexerState.LexerReady;
                                            if (line.Peek().Kind != GreaterThan)
                                            {
                                                // TODO: Error
                                            }
                                        }
                                        if (filename != null)
                                        {
                                            // Storing locally here because of the closure in delegate
                                            var localBuffer = tokenBuffer;
                                            var pipeline = IncludePipeline();
                                            pipeline.StreamAndChain(FilterEof!).Chain(localBuffer);
                                            // The task doesn't need to be awaited.
                                            var task = Task.Run(
                                                () => ProcessPipeline(filename, pipeline));
                                        }
                                    }
                                }
                                finally
                                {
                                    TranslationUnit.LexerState = LexerState.LexerReady;
                                }
                                break;
                            default:
                                tokenBuffer.Complete();
                                break;
                        }
                    }
                    else
                    {
                        tokenBuffer.Complete();
                    }
                }
                else
                {
                    foreach (var token in line)
                    {
                        tokenBuffer.Post(token);
                    }
                    tokenBuffer.Complete();
                }
            }
        }

        protected async IAsyncEnumerable<Token> AssembleBuffers(IAsyncStream<BufferBlock<Token>> lines)
        {
            await foreach (var line in lines)
            {
                await foreach (var token in line.ReceiveAllAsync())
                {
                    yield return token;
                }
            }
        }

        protected IAsyncStream<Token> Preprocess()
        {
            var pipeline = FullPipeline();

            var pipelineTask = ProcessPipeline(Reader, pipeline);

            return pipeline.ToStream()!;
        }

        protected IAsyncStream<Token> TokenizeOnly()
        {
            var pipeline = BasicPipeline(false);

            var pipelineTask = ProcessPipeline(Reader, pipeline);

            return pipeline.ToStream()!;
        }

        protected async IAsyncEnumerable<Token> DoPreprocess(IAsyncStream<IStream<Token>> lines)
        {
            await foreach (var line in lines)
            {
                if (line.Peek().Kind == Pound)
                {
                    line.Read();
                    if (line.Peek().Kind == Identifier)
                    {
                        switch ((line.Read() as ValueToken<string>)!.Value.ToLower())
                        {
                            case "define":
                                if (line.Peek().Kind == Identifier)
                                {
                                    var ident = (ValueToken<string>)line.Read();
                                    var list = new List<Token>();
                                    foreach (var token in line)
                                    {
                                        list.Add(token);
                                    }
                                    var define = new DefineSymbol(SymbolType.DefineSymbol,
                                        ident!.Value, list);
                                    TranslationUnit.Defines.Add(ident.Value, define);
                                }
                                break;
                            case "undef":
                                if (line.Peek().Kind == Identifier)
                                {
                                    var ident = (ValueToken<string>)line.Read();
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
                else
                {
                    foreach (var token in line)
                    {
                        yield return token;
                    }
                }
            }
        }

        protected async IAsyncEnumerable<IStream<Token>> CollectLines(IAsyncStream<Token> input)
        {
            while (!await input.Eof())
            {
                var list = new List<Token>();
                await foreach (var token in ScanLine(input))
                {
                    list.Add(token);
                }
                yield return new StreamWrapper<Token>(list);
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

        async IAsyncEnumerable<Token> MaybeSubstitute(ValueToken<string> t)
        {
            if (TranslationUnit.Defines.ContainsKey(t.Value))
            {
                await foreach (var define_token in Substitute(t))
                {
                    yield return define_token;
                }
            }
            else
            {
                yield return t;
            }
        }

        async IAsyncEnumerable<Token> Substitute(ValueToken<string> t)
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
                                    await foreach (var substitution_token in MaybeSubstitute(string_token))
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