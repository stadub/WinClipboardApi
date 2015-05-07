using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Utils
{
    public static class EnumerableHelpers
    {
        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> enumerable,Action<T> action)
        {
            var list=enumerable as List<T>;
            if (list != null)
            {
                list.ForEach(action);
                return;
            }
            foreach (var item in enumerable)
                action(item);
        }
    }
}
