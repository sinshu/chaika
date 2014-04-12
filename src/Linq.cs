using System;
using System.Collections.Generic;
using System.Linq;

namespace Ore.Chaika
{
    public static class Linq
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] second)
        {
            return Enumerable.Concat(first, second);
        }
    }
}
