using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClipboardHelper.Watcher;
using ClipboardHelper.Win32;

namespace ClipboardHelperTest
{
    internal class WinformWrapper : IDisposable,IClipbordMessageProvider
    {
        private FormWrapper form;
        private bool disposed;

        public void CreateWindow()
        {
            if (form == null)
            {
                form = new FormWrapper();
            }
            if (form.Visible)
                    return;
                form.Show();
                form.GotFocus += form_GotFocus;
        }

        void form_GotFocus(object sender, EventArgs e)
        {
           
        }

        public void CloseWindow()
        {
            if(form!=null)
                form.Close();
        }
        private class FormWrapper : Form
        {
            public Predicate<Message> WinProcHandler { get; set; }
            protected override void WndProc(ref Message m)
            {
                if (WinProcHandler != null)
                {
                    WinProcHandler(m);

                }
                base.WndProc(ref m);
            }
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
                    if (form != null)
                    {
                        var tempForm = form;
                        form = null;
                        tempForm.Close();
                        tempForm.Dispose();
                        
                    }
                }
                disposed = true;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_COPYDATA = 0x004A;

        public void SendCopyDataMessage(IntPtr hWnd,IntPtr wParam, IntPtr lParam)
        {
            SendMessage(hWnd,WM_COPYDATA, wParam, lParam);
        }

        public IntPtr WindowHandle
        {
            get { return form.Handle; }
        }
    }
}
