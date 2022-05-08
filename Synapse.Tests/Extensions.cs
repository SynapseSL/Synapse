using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Tests
{
    internal static class Extensions
    {
        internal static IEnumerable<TSource> TakeLast<TSource>(this IReadOnlyList<TSource> source, int count)
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
