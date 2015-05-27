using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    static class Logger
    {
        public static void LogError(string error)
        {
            var t = new StackTrace(true);

            Trace.Write(string.Format("StackTrace:{1}\r\nError:{0}", error, t.ToString()));
        }
        public static void LogError(string methodName,string error)
        {
            Trace.Write(string.Format("MethodCall:{1}\r\nError:{0}", error, methodName));
        }
    }
}
