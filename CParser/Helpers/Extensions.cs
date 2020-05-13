using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Threading;
using System.Runtime.CompilerServices;
using System;
using System.IO;

namespace CParser.Helpers
{
    public static class Extensions
    {
        const int BUFFER_SIZE = 1024;
        public async static Task<List<T>> AsList<T>(this IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();
            await foreach (var item in enumerable)
            {
                list.Add(item);
            }
            return list;
        }
        public async static Task<IEnumerable<T>> AsEnumerable<T>(this IAsyncEnumerable<T> enumerable)
        {
            return await enumerable.AsList();
        }

        // TODO: Wait until this becomes a part of the API
        // https://github.com/dotnet/corefx/issues/41125
        public async static IAsyncEnumerable<TOutput> ReceiveAllAsync<TOutput>(this ISourceBlock<TOutput> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await source.OutputAvailableAsync(cancellationToken))
            {
                yield return source.Receive();
            }
        }

        public static AsyncStreamFunc<TInput, TOutput> AsFunction<TInput, TOutput>(this IPropagatorBlock<TInput, TOutput> source, TOutput sentinel = default)
        {
            var wrapper = new AsyncStreamWrapper<TOutput>(source.ReceiveAllAsync(), sentinel);
            return input => {
                var dummyTask = source.PostAllAsync(input)
                    .ContinueWith(_ => source.Complete());
                return wrapper;
            };
        }

        public static IAsyncStream<char> ToStream(this TextReader source, CancellationToken cancellationToken = default)
        {
            var buffer = new BufferBlock<char>();
            var dummyTask = buffer.PostAllTextAsync(source, cancellationToken)
                    .ContinueWith(_ => buffer.Complete());
            return new AsyncStreamWrapper<char>(buffer.ReceiveAllAsync(cancellationToken));
        }

        public static IAsyncStream<TOutput> ToStream<TOutput>(this ISourceBlock<TOutput> source, TOutput sentinel = default, CancellationToken cancellationToken = default)
        {
            return new AsyncStreamWrapper<TOutput>(source.ReceiveAllAsync(cancellationToken), sentinel);
        }

        public static IAsyncStream<TOutput> ToStream<TOutput>(this IAsyncEnumerable<TOutput> source, TOutput sentinel = default, CancellationToken cancellationToken = default)
        {
            return new AsyncStreamWrapper<TOutput>(source, sentinel);
        }

        // TODO: Wait until this becomes a part of the API
        // https://github.com/dotnet/corefx/issues/41125
        public async static Task PostAllAsync<TInput>(this ITargetBlock<TInput> target, IAsyncEnumerable<TInput> source, CancellationToken cancellationToken = default)
        {
            await foreach (var item in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                target.Post(item);
            }
        }

        public async static IAsyncEnumerable<char> AsAsyncEnumerable(this TextReader source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int count;
            var buffer = new char[BUFFER_SIZE];
            while ((count = await source.ReadAsync(buffer, 0, BUFFER_SIZE)) > 0)
            {
                foreach(var c in buffer)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return c;
                }
            }
        }

        public async static Task PostAllTextAsync(this ITargetBlock<char> target, TextReader source, CancellationToken cancellationToken = default)
        {
            var buffer = new char[BUFFER_SIZE];
            int count;
            while ((count = await source.ReadAsync(buffer, 0, BUFFER_SIZE)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (var i = 0; i < count; i++)
                {
                    target.Post(buffer[i]);
                }
            }
        }

        public static IPropagatorBlock<TInput, TOutput> Buffered<TInput, TOutput>(this AsyncStreamFunc<TInput, TOutput> func, TInput sentinel = default, CancellationToken cancellationToken = default)
        {
            return new AsyncStreamBlock<TInput, TOutput>(func, sentinel, cancellationToken);
        }

        // Chains blocks fluently and propagates completion by default.
        public static IPropagatorBlock<TInput, TOutput> Chain<TInput, TLink, TOutput>(this IPropagatorBlock<TInput, TLink> source, IPropagatorBlock<TLink, TOutput> target, DataflowLinkOptions? options = null)
        {
            source.LinkTo(
                    target, 
                    options ?? new DataflowLinkOptions { PropagateCompletion = true });
            return DataflowBlock.Encapsulate<TInput, TOutput>(source, target);
        }

        public static IAsyncStream<TOutput> Chain<TInput, TOutput>(this IAsyncStream<TInput> source, AsyncStreamFunc<TInput, TOutput> target, TOutput sentinel = default, CancellationToken cancellationToken = default)
        {
            return target(source).ToStream(sentinel, cancellationToken);
        }

        public static AsyncStreamFunc<TInput, TOutput> Chain<TInput, TLink, TOutput>(this AsyncStreamFunc<TInput, TLink> func1, AsyncStreamFunc<TLink, TOutput> func2, TLink sentinel = default)
        {
            return input => func2(new AsyncStreamWrapper<TLink>(func1(input), sentinel));
        }

        public static IAsyncStream<T> ComposeAndChain<T>(this IAsyncStream<T> source, params AsyncStreamFunc<T, T>[] functions)
        {
            return functions.Aggregate(
                                source, 
                                (stream, f) => stream.Chain(f));
        }

        // Chains a block to a new async stream block based on the given function, and propagates completion by default.
        public static IPropagatorBlock<TInput, TOutput> BufferAndChain<TInput, TLink, TOutput>(this IPropagatorBlock<TInput, TLink> source, AsyncStreamFunc<TLink, TOutput> func, DataflowLinkOptions? options = null, TLink sentinel = default, CancellationToken cancellationToken = default)
        {
            return source.Chain(
                            func.Buffered(sentinel, cancellationToken),
                            options);
        }

        public static IPropagatorBlock<TInput, TOutput> ComposeAndChain<TInput, TOutput>(this IPropagatorBlock<TInput, TOutput> source, DataflowLinkOptions? options, params AsyncStreamFunc<TOutput, TOutput>[] functions)
        {
            return functions.Aggregate(
                                source, 
                                (block, f) => block.BufferAndChain(f, options));
        }

        public static IPropagatorBlock<TInput, TOutput> ComposeAndChain<TInput, TOutput>(this IPropagatorBlock<TInput, TOutput> source, params AsyncStreamFunc<TOutput, TOutput>[] functions)
        {
            return source.ComposeAndChain(null, functions);
        }
    }
}