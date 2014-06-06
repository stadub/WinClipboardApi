using System;
using System.Runtime.InteropServices;

namespace ClipboardHelper.Win32
{
    public sealed class ExceptionHelpers
    {
        public static Exception GetLastWin32Exception()
        {
            //return new System.ComponentModel.Win32Exception();
            var win32Error = Marshal.GetHRForLastWin32Error();
            return Marshal.GetExceptionForHR(win32Error);
        }

        public static void GuardZeroHandle(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
        }

        public static void GuardZeroHandle(UIntPtr ptr)
        {
            if (ptr == UIntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
        }
    }
}
