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
            while (!source.Completion.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                TOutput item = default!;
                try
                {
                    item = await source.ReceiveAsync();
                }
                catch (TaskCanceledException) {}
                if (item != null)
                {
                    yield return item;
                }
                else break;
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
                if (cancellationToken.IsCancellationRequested) break;
                target.Post(item);
            }
        }
        public async static Task PostAllTextAsync(this ITargetBlock<char> target, TextReader source, CancellationToken cancellationToken = default)
        {
            var transform = new TransformManyBlock<IEnumerable<char>, char>(
                input => input,
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken }
            );
            using (var link = transform.LinkTo(
                target, new DataflowLinkOptions { PropagateCompletion = true }))
            {
                var buffer = new char[BUFFER_SIZE];
                int count;
                while (!cancellationToken.IsCancellationRequested &&
                    (count = await source.ReadAsync(buffer, 0, BUFFER_SIZE)) > 0)
                {
                    transform.Post(buffer.Take(count));
                    buffer = new char[BUFFER_SIZE]; // Send new pages of data as they become available
                }
                transform.Complete();
            }
        }
    }
}