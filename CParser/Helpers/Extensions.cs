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
        public async static Task<IEnumerable<T>> AsEnumerable<T>(this IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();
            await foreach (var item in enumerable)
            {
                list.Add(item);
            }
            return list;
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

        public static IAsyncStream<TOutput> ToStream<TOutput>(this ISourceBlock<TOutput> source, TOutput sentinel = default, CancellationToken cancellationToken = default)
        {
            return new AsyncStreamWrapper<TOutput>(source.ReceiveAllAsync(cancellationToken), sentinel);
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
            var transform = new TransformManyBlock<IEnumerable<char>, char>(
                input => input,
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken }
            );
            using (var link = transform.LinkTo(target))
            {
                var buffer = new char[BUFFER_SIZE];
                int count;
                while ((count = await source.ReadAsync(buffer, 0, BUFFER_SIZE)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    transform.Post(buffer.Take(count));
                    buffer = new char[BUFFER_SIZE]; // Send new pages of data as they become available
                }
            }
        }

        public static IPropagatorBlock<TInput, TOutput> AsBlock<TInput, TOutput>(this AsyncStreamFunc<TInput, TOutput> func, TInput sentinel = default, CancellationToken cancellationToken = default)
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

        // Chains a block to a new async stream block based on the given function, and propagates completion by default.
        public static IPropagatorBlock<TInput, TOutput> StreamAndChain<TInput, TLink, TOutput>(this IPropagatorBlock<TInput, TLink> source, AsyncStreamFunc<TLink, TOutput> func, DataflowLinkOptions? options = null, TLink sentinel = default, CancellationToken cancellationToken = default)
        {
            return source.Chain(
                            func.AsBlock(sentinel, cancellationToken),
                            options);
        }

        public static IPropagatorBlock<T, T> ComposeAndChain<T>(this ISourceBlock<T> source, DataflowLinkOptions? options, params AsyncStreamFunc<T, T>[] functions)
        {
            return functions.Aggregate(
                                (IPropagatorBlock<T, T>)new BufferBlock<T>(), 
                                (block, f) => block.StreamAndChain(f, options));
        }

        public static IPropagatorBlock<T, T> ComposeAndChain<T>(this ISourceBlock<T> source, params AsyncStreamFunc<T, T>[] functions)
        {
            return source.ComposeAndChain(null, functions);
        }
    }
}