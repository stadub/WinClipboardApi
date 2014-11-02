using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardHelper.Win32
{
    class ClipbordWatcherWindow:ApiWindow
    {
        public ClipbordWatcherWindow()
            : base("ClipbordWatcher")
        {
        }

        public new IntPtr WindowHandle { get{return base.WindowHandle;}} 

        private const int PM_REMOVE = 0x0001;
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        public EventHandler<EventArgs> ClipboardContentChanged; 

        protected override IntPtr WindowProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                case WM_CLIPBOARDUPDATE:
                    if (ClipboardContentChanged != null) 
                        ClipboardContentChanged(this,new EventArgs());
                    return IntPtr.Zero;
            }
            return base.WindowProc(hwnd, uMsg, wParam, lParam);
        }
    }

    public class ClipbordWatcher:IDisposable
    {
        private ClipbordWatcherWindow watcherWindow;
        public ClipbordWatcher()
        {
            watcherWindow= new ClipbordWatcherWindow();
            watcherWindow.RegisterWindowClass();
            watcherWindow.ClipboardContentChanged += (sender, args) => ClipboardContentChanged();
            waitHandle= new ManualResetEvent(false);
        }

        private ManualResetEvent waitHandle;
        public IEnumerable<uint> WaitClipboardData()
        {
            while (!disposed)
            {
                WaitHandle.WaitAny(new WaitHandle[] {waitHandle});
                yield return Clipboard.SequenceNumber;
            }
        } 

        private void ClipboardContentChanged()
        {
            waitHandle.Set();
        }

        private Task watcherTask;
        public void Create()
        {
            if(watcherTask!=null)
                return;
            watcherWindow.CreateWindow();

            Thread tr= new Thread(() => watcherWindow.ShowWindow());
            tr.SetApartmentState(ApartmentState.STA);
            tr.Start();
            //watcherTask = Task.Factory.StartNew(() => watcherWindow.ShowWindow(),
            //    CancellationToken.None,
            //    TaskCreationOptions.None,
            //    TaskScheduler.Current
            //    //TaskScheduler.FromCurrentSynchronizationContext()
            //);

            if (!ClipboardListener.AddClipboardFormatListener(watcherWindow.WindowHandle))
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError);
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)return;
            disposed = true;
            waitHandle.Set();
            waitHandle.Dispose();
            if (watcherTask != null)
            {
                watcherTask.Dispose();
                ClipboardListener.RemoveClipboardFormatListener(watcherWindow.WindowHandle);
            }
            watcherWindow.Dispose();

        }
    }
}
