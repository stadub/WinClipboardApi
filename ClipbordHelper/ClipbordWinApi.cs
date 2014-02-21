using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipbordHelper
{

    public class ClipboardWinApi : IDisposable
    {
        private bool disposed;

        private static readonly IntPtr notOwned = new IntPtr(-1);

        private IntPtr clipbordOwner;
        public ClipboardWinApi()
        {
            clipbordOwner=new IntPtr(-1);
        }

        public void Open()
        {
            this.Open(IntPtr.Zero);
        }

        public void Open(IntPtr hWnd)
        {
            if (clipbordOwner != notOwned)
            {
                throw new ClipbordOpenedException("Clipbord allready opened");
            }
            var opened = OpenClipboard(hWnd);
            if (!opened)
            {
                var errcode = Marshal.GetLastWin32Error();
                var innerException=Marshal.GetExceptionForHR(errcode);
                throw new OpenClipbordException(innerException);
            }
            clipbordOwner = hWnd;
        }


        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();



        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        private static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        private static extern int GetClipboardFormatName(uint format,
            [Out][MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFormatName, int cchMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        private static extern int GetPriorityClipboardFormat(UIntPtr paFormatPriorityList, int cFormats);


#if WINNT_VISTA

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool GetUpdatedClipboardFormats([Out]UIntPtr lpuiFormats, uint cFormats, [Out] UIntPtr pcFormatsOut);

#endif

        public void Close()
        {
            if (clipbordOwner == notOwned)
                return;
            Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
                disposed = true;
            }
            if (clipbordOwner != notOwned)
                CloseClipboard();
        }
    }
}
