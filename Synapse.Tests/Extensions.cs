using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Tests
{
    public static class Extensions
    {
        public static IEnumerable<TSource> TakeLast<TSource>(this IReadOnlyList<TSource> source, int count)
        {
            var sourceCount = source.Count();
            var beginning = sourceCount - 1;
            var ending = Math.Max(sourceCount - count, 0);

            for (var i = beginning; i >= ending; i--)
            {
                yield return source[i];
            }
        }
    }
}
