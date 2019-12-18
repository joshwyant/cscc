using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace CParser.Helpers
{
    public static class Functions
    {
        public static IPropagatorBlock<T, T> Compose<T>(params AsyncStreamFunc<T, T>[] functions)
        {
            return Compose(default!, functions);
        }
        public static IPropagatorBlock<T, T> Compose<T>(T sentinel, params AsyncStreamFunc<T, T>[] functions)
        {
            return functions.Aggregate(
                    (IPropagatorBlock<T, T>)new BufferBlock<T>(), 
                    (block, func) => block.StreamAndChain(func, null, sentinel));
        }
    }
}