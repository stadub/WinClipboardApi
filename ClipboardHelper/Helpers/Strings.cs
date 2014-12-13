using System;

namespace ClipboardHelper.Helpers
{
    public static class Strings
    {
        public static Tuple<string, string> SplitString(this string str, char separator)
        {
            var index = str.IndexOf(separator);
            var str2 = str.Length > index?str.Substring(index + 1):string.Empty;
            var str1 = str.Substring(0, index);
            return new Tuple<string, string>(str1, str2);
        }
    }
}
