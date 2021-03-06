using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CParser.Helpers
{
    public class StreamWrapper<T> : IStream<T>
    {
        private IEnumerable<T> Source { get; }
        private IEnumerator<T> SourceEnumerator { get; }
        private Stack<T> Buffer { get; }
        public T Sentinel { get; }

        public StreamWrapper(IEnumerable<T> source, T sentinel = default)
        {
            Source = source;
            Buffer = new Stack<T>();
            Sentinel = sentinel;
            SourceEnumerator = source.GetEnumerator();
        }

        protected IEnumerable<T> Stream()
        {
            while (Buffer.Any() || SourceEnumerator.MoveNext())
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

        public T Peek()
        {
            return Eof() ? Sentinel : Buffer.Peek();
        }

        public T Read()
        {
            return Eof() ? Sentinel : Buffer.Pop();
        }

        public bool Eof()
        {
            if (Buffer.Any())
            {
                return false;
            }
            if (SourceEnumerator.MoveNext())
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

        public IEnumerator<T> GetEnumerator()
        {
            return Stream().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }
    }
}