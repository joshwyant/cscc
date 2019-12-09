using System.Collections.Generic;
using System.Threading.Tasks;

namespace CParser.Helpers
{
    public interface IAsyncStream<T> : IAsyncEnumerable<T>
    {
        T Sentinel { get; }
        void PutBack(T val);

        Task<T> Peek();

        Task<T> Read();

        Task<bool> Eof();
    }
}