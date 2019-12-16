using System.Collections.Generic;

namespace CParser.Helpers
{
    public delegate IAsyncEnumerable<TOutput> AsyncStreamFunc<TInput, TOutput>(IAsyncStream<TInput> stream);
}