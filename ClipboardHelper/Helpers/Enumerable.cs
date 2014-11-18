using System;
using System.Collections.Generic;

namespace ClipboardHelper.Helpers
{
    public static class Enumerable
    {
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
