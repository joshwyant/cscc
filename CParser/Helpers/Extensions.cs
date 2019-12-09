using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CParser.Helpers
{
    public static class Extensions
    {
        public async static Task<IEnumerable<T>> AsEnumerable<T>(this IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();
            await foreach (var item in enumerable)
            {
                list.Add(item);
            }
            return list;
        }
    }
}