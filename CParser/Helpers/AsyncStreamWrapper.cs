using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CParser.Helpers
{
    public class AsyncStreamWrapper<T> : IAsyncStream<T>
    {
        private IAsyncEnumerable<T> Source { get; }
        private IAsyncEnumerator<T> SourceEnumerator { get; }
        private Stack<T> Buffer { get; }
        public T Sentinel { get; }

        public AsyncStreamWrapper(IAsyncEnumerable<T> source, T sentinel = default)
        {
            Source = source;
            Buffer = new Stack<T>();
            Sentinel = sentinel;
            SourceEnumerator = source.GetAsyncEnumerator();
        }

        protected async IAsyncEnumerable<T> Stream()
        {
            while (Buffer.Any() || await SourceEnumerator.MoveNextAsync())
            {
                if (Buffer.Any())
                {
                    while (Buffer.Any())
                    {
                        yield return Buffer.Pop();
                    }
                }
                else
                {
                    yield return SourceEnumerator.Current;
                }
            }
        }

        public void PutBack(T val)
        {
            Buffer.Push(val);
        }

        public async Task<T> Peek()
        {
            return await Eof() ? Sentinel : Buffer.Peek();
        }

        public async Task<T> Read()
        {
            return await Eof() ? Sentinel : Buffer.Pop();
        }

        public async Task<bool> Eof()
        {
            if (Buffer.Any())
            {
                return Buffer.Peek()!.Equals(Sentinel);
            }
            if (await SourceEnumerator.MoveNextAsync())
            {
                if (SourceEnumerator.Current!.Equals(Sentinel))
                {
                    return true;
                }
                PutBack(SourceEnumerator.Current);
                return false;
            }
            return true;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return Stream().GetAsyncEnumerator();
        }
    }
}