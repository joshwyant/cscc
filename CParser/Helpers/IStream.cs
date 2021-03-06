using System.Collections.Generic;
using System.Threading.Tasks;

namespace CParser.Helpers
{
    public interface IStream<T> : IEnumerable<T>
    {
        T Sentinel { get; }
        void PutBack(T val);

        T Peek();

        T Read();

        bool Eof();
    }
}