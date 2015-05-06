using System;
using System.Diagnostics;

namespace Utils.Helpers
{
    class Debugger
    {
        public static void Assert(Func<bool> that, string message)
        {
#if DEBUG
            Debug.Assert(that(), message);
#endif
        }
    }
}
