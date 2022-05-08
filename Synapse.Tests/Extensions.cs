using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Tests
{
    public static class Extensions
    {
        public static IEnumerable<TSource> TakeLast<TSource>(this IReadOnlyList<TSource> source, int count)
        {
            int sourceCount = source.Count();
            int beginning = sourceCount - 1;
            int ending = Math.Max(sourceCount - count, 0);

            for (int i = beginning; i >= ending; i--)
            {
                yield return source[i];
            }
        }
    }
}
