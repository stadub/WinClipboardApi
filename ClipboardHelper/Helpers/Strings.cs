using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardHelper.Helpers
{
    public static class Strings
    {
        public static Tuple<string, string> SplitString(this string str, char separator)
        {
            var index = str.IndexOf(separator);
            var str1 = str.Substring(index);
            var str2 = str.Substring(0, index);
            return new Tuple<string, string>(str1, str2);
        }
    }
}
