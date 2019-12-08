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

namespace CParser.Preprocessing
{
    class Preprocessor : IStream<Token>
    {
        private const int TAB_WIDTH = 8;
        public TranslationUnit TranslationUnit { get; }
        protected CharacterStream RawStream { get; }
        protected IStream<Token> OutputStream { get; }
        protected Lexer Lexer { get; }
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
            TranslationUnit = tu;
            RawStream = new CharacterStream(reader);
            Lexer = TokenizeStream(RawStream, preprocess);
            if (preprocess)
            {
                OutputStream = new StreamWrapper<Token>(Preprocess(Lexer));
            }
            else
            {
                OutputStream = Lexer;
            }
        }
        public static Func<IStream<char>, IAsyncEnumerable<char>> Compose(
            Func<IStream<char>, IAsyncEnumerable<char>> func1, Func<IStream<char>, IAsyncEnumerable<char>> func2)
        {
            return x => new StreamWrapper<char>(func1(new StreamWrapper<char>(func2(x))));
        }

        // Tokenizes a stream for a source file or include file.
        protected Lexer TokenizeStream(IStream<char> input, bool preprocess)
        {
            Func<IStream<char>, IAsyncEnumerable<char>>[] pipeline =
            { 
                CountLines,
                ReplaceTrigraphs,
                SpliceLines,
            };

            var transform = pipeline.Reverse()
                .Aggregate((func1, func2) => 
                    (x => new StreamWrapper<char>(func1(new StreamWrapper<char>(func2(x))))));

            return new Lexer(TranslationUnit, (IStream<char>)transform(RawStream), preprocess, preprocess);
        }

        protected async IAsyncEnumerable<char> CountLines(IStream<char> input)
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

        protected async IAsyncEnumerable<char> ReplaceTrigraphs(IStream<char> input)
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
                        while (num_question_marks > 2)
                        {
                            yield return '?';
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
                        break;
                }
            }
            while (num_question_marks > 0)
            {
                yield return '?';
            }
        }

        protected async IAsyncEnumerable<char> SpliceLines(IStream<char> input)
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

        protected /*async*/ IAsyncEnumerable<Token> Preprocess(IStream<Token> pre_input)
        {
            throw new NotImplementedException();
            // var input = new StreamWrapper<Token>(ProcessIncludes(pre_input));
            // input.GetAsyncEnumerator().MoveNextAsync().
            // await foreach (var token in input)
            // {
            //     switch (token)
            //     {
            //         case Token t when t.Kind == Pound:
            //             // TODO: Process the macro
            //             break;
            //         case ValueToken<string> t when t.Kind == Identifier:
            //             await foreach (var substitution_token in MaybeSubstitute(t))
            //             {
            //                 yield return substitution_token;
            //             }
            //             break;
            //     }
            // }
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