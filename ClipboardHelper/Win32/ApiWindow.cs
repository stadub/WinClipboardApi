using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClipboardHelper.WinApi;
using ClipboardHelper.WinApi.WindowEnums;

namespace ClipboardHelper.Win32
{
    public class ApiWindow:IDisposable
    {
        private readonly string windowClass;
        private IntPtr hInstance;
        private UInt16 regRest;

        public ApiWindow(string windowClass)
        {
            // string szName = "MyWinClass";
            this.windowClass = windowClass;
            hInstance = System.Diagnostics.Process.GetCurrentProcess().Handle;
        }

       
        protected virtual IntPtr WindowProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                case WM_DESTROY:
                    Window.PostQuitMessage(0);
                    return IntPtr.Zero;

                case WM_PAINT:
                    {
                        Gdi.PAINTSTRUCT ps;
                        var hdc = Gdi.BeginPaint(hwnd, out ps);
                        const int ColorBlack = 0;
                        IntPtr brush = Gdi.CreateSolidBrush((uint)ColorBlack);
                        Gdi.FillRect(hdc, ref ps.rcPaint, brush);
                        Gdi.DeleteObject(brush);
                        Gdi.EndPaint(hwnd, ref ps);
                    }
                    return IntPtr.Zero;

            }
            return Window.DefWindowProc(hwnd, uMsg, wParam, lParam);
        }

        private Window.WNDCLASSEX CreateWindowClass()
        {

            Window.WNDCLASSEX wndClass = new Window.WNDCLASSEX();
            wndClass.cbSize = Marshal.SizeOf(typeof(Window.WNDCLASSEX));

            wndClass.style = (int)(ClassStyles.HorizontalRedraw | ClassStyles.VerticalRedraw);

            wndClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate((Window.WndProc)(WindowProc));

            wndClass.cbClsExtra = 0;
            wndClass.cbWndExtra = 0;
            wndClass.hInstance = hInstance;
            //wndClass.hCursor = WinAPI.LoadCursor(IntPtr.Zero, (int)IdcStandardCursor.IDC_ARROW);
            wndClass.lpszMenuName = null;
            wndClass.lpszClassName = windowClass;

            //WindowStyleEx.WS_EX_OVERLAPPEDWINDOW
            //ushort regResult = (ushort)WinAPI.RegisterClassEx(ref wndClass); // change the varie RegisterClassEx2

            return wndClass;
        }

        public void RegisterWindowClass()
        {
            var wndClass = CreateWindowClass();
            regRest = Window.RegisterClassEx(ref wndClass);
            if (regRest == 0)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError);
            }
        }


        private const int UseDefault = -1;

        protected IntPtr WindowHandle;

        public void CreateWindow()
        {

            WindowHandle = Window.CreateWindowEx2(0, regRest, "The hello proram", WindowStyles.WS_OVERLAPPEDWINDOW,
                UseDefault, UseDefault, UseDefault, UseDefault,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);


            if (WindowHandle == IntPtr.Zero)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError);
            }
        }
        public void ShowWindow(){
            Window.ShowWindow(WindowHandle, ShowWindowCommands.Normal);
            Window.UpdateWindow(WindowHandle);
            Window.MSG msg;
            while (Window.GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
            {
                Window.TranslateMessage(ref msg);
                Window.DispatchMessage(ref msg);
            }			
        }
        const uint WM_PAINT = 0xf ;
        const uint WM_DESTROY = 0x0002;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
            Window.UnregisterClass(windowClass, hInstance);
        }
        ~ApiWindow()
        {
            Dispose(false);
        }
    }
}
