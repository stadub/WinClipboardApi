using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipbordHelper
{
    public class ClipboardWinApi
    {

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseClipboard();
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();



        [DllImport("user32.dll")]
        public static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        public static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        public static extern int GetClipboardFormatName(uint format,
            [Out][MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFormatName, int cchMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        public static extern int GetPriorityClipboardFormat(UIntPtr paFormatPriorityList, int cFormats);


#if WINNT_VISTA

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool GetUpdatedClipboardFormats([Out]UIntPtr lpuiFormats, uint cFormats, [Out] UIntPtr pcFormatsOut);

#endif

       
    }
}
