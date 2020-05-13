using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace CParser.Helpers
{
    public static class Functions
    {
        public static IPropagatorBlock<T, T> ComposeBuffer<T>(params AsyncStreamFunc<T, T>[] functions)
        {
            return ComposeBuffer(default!, functions);
        }
        public static IPropagatorBlock<T, T> ComposeBuffer<T>(T sentinel, params AsyncStreamFunc<T, T>[] functions)
        {
            return functions.Aggregate(
                    (IPropagatorBlock<T, T>)new BufferBlock<T>(), 
                    (block, func) => block.BufferAndChain(func, null, sentinel));
        }
        public static AsyncStreamFunc<T, T> Compose<T>(params AsyncStreamFunc<T, T>[] functions)
        {
            return Compose(default!, functions);
        }
        public static AsyncStreamFunc<T, T> Compose<T>(T sentinel, params AsyncStreamFunc<T, T>[] functions)
        {
            return functions.Aggregate(
                    (func1, func2) => input => func2(new AsyncStreamWrapper<T>(func1(input), sentinel)));
        }
    }
}