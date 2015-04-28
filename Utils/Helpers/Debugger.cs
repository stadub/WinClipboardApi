using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
