using System;
using System.Collections.Generic;
using System.Linq;

namespace UnusedAssetsFinder.Editor.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable"/> types
    /// </summary>
    public static class OrderedEnumerableExtensions
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return source.OrderBy(selector);

            return source.OrderByDescending(selector);
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return source.ThenBy(selector);

            return source.ThenByDescending(selector);
        }
    }
}
