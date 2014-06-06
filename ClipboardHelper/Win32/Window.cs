using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardHelper.Win32
{
    class Window
    {
        private void CreateWindow()
        {
            var hwnd = CreateWindowEx(
                WS_EX_NOPARENTNOTIFY | WS_EX_TRANSPARENT,
                "DUMMY_CLASS", "dummy_name", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            

            if (hwnd == IntPtr.Zero)
            {
                //MessageBox(NULL, "Window Creation Failed!", "Error!",
                //    MB_ICONEXCLAMATION | MB_OK);
                return;
            }

            ShowWindow(hwnd, 0);
            UpdateWindow(hwnd);

            MSG Msg;

            Task.Factory.StartNew(
                () =>
                {
                    // Step 3: The Message Loop
                    while (GetMessage(out Msg, hwnd, 0, 0) > 0)
                    {
                        TranslateMessage(ref Msg);
                        DispatchMessage(ref Msg);
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
            
        }
        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool TranslateMessage([In] ref MSG lpMsg);
        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref MSG lpmsg);
        [DllImport("user32.dll")]
        static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
           uint wMsgFilterMax);


        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT p;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PeekMessage(out NativeMessage lpMsg, HandleRef hWnd, uint wMsgFilterMin,
           uint wMsgFilterMax, uint wRemoveMsg);


        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public UInt32 message;
            public IntPtr wParam;
            public IntPtr lParam;
            public UInt32 time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        public static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        private int WS_EX_NOPARENTNOTIFY = 0x00000004;
        private int WS_EX_TRANSPARENT = 0x00000020;
        private int WS_CHILD = 0x40000000;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(
           int dwExStyle,
           string lpClassName,
           string lpWindowName,
           int dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }
}
