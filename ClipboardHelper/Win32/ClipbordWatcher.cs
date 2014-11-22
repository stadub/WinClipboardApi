using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClipboardHelper.Helpers;
using ClipboardHelper.Win32.ClipbordWatcherTypes;

namespace ClipboardHelper.Win32
{
    public struct CopyData
    {
        public IntPtr Sender;
        public UIntPtr PointerData;
        public long Data;
        public IntPtr VoidData;

    }
    public class ClipbordWatcher:IDisposable
    {
        private const string WatcherProcessName = "ClipboardWatcher.exe";
        public EventHandler<EventArgs> OnClipboardContentChanged;
        public EventHandler<EventArgs> OnClipboardContentDestroy;
        public EventHandler<EventArgs<int>> OnRenderFormatRequested;
        public EventHandler<EventArgs<CopyData>> OnClipboarCopyDataSent;
        public EventHandler<EventArgs<IntPtr>> OnMessageWindowHwndReceived;

        private IntPtr windowHandle;
        private ManualResetEvent waitHandle;
        private Process proc;

        private Task watcherTask;

        public ClipbordWatcher()
        {
            OnClipboardContentChanged += ClipboardContentChanged;
            waitHandle = new ManualResetEvent(false);
        }

        public static bool WatcherAllreadyRan()
        {
            return Process.GetProcessesByName(WatcherProcessName).Any();
        }

        public void Start(bool throwIfAnotherListenerStarted = false)
        {
            if (proc != null) throw new ClipbordWatcherException("Cannot start second listener istance");

            if (throwIfAnotherListenerStarted && WatcherAllreadyRan())
                throw new ClipbordWatcherException(
                    "Other instance of the Clipboard watcher already started. Close another one first");

            watcherTask=Task.Factory.StartNew(() =>
            {
                StartMessageWindow();
                StartListenLoop();
            });
        }


        protected virtual string CreateClassName()
        {
            var classNameBuilder= new StringBuilder(AppDomain.CurrentDomain.FriendlyName);
            for (int i = 0; i < classNameBuilder.Length; i++)
            {
                char c = classNameBuilder[i];
                if (!Char.IsLetterOrDigit(c))
                    classNameBuilder[i] = '_';
            }
            return classNameBuilder.ToString();
        }

        private void StartMessageWindow()
        {

            var procStartInfo = new ProcessStartInfo(WatcherProcessName);
            //procStartInfo.CreateNoWindow = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardInput = true;
            procStartInfo.UseShellExecute = false;

            procStartInfo.Arguments = CreateClassName();
            proc = Process.Start(procStartInfo);
        }

        protected void StartListenLoop(){
            string line;
            while ((line=proc.StandardOutput.ReadLine())!=null)
            {
                Tuple<string, string> data = line.SplitString(':');

                HandleListenMessage(data.Item1, data.Item2);
            }
        }

        protected virtual void HandleListenMessage(string type,string data)
        {
            MsgSeverity severity;
            Enum.TryParse(type, true, out severity);

            switch (severity)
            {
                case MsgSeverity.Error:
                    ThrowError(data);
                    break;
                case MsgSeverity.Warning:
                    WriteWarning(data);
                    break;
                case MsgSeverity.Info:
                    WriteInfo(data);
                    break;
                case MsgSeverity.AppData:
                    UpdateAppState(data);
                    break;
                case MsgSeverity.Debug:
                    WriteDebug(data);
                    break;
            }
        }

        protected virtual void ThrowError(string errorMessage)
        {
            var errData = errorMessage.SplitString(':');
            var errCodeStr = errData.Item1.Replace("ErrCode", "");
            var errCode = Int32.Parse(errCodeStr);
            Marshal.ThrowExceptionForHR(errCode);
        }

        protected virtual void WriteInfo(string infoMsg)
        {
            Debug.Print(infoMsg);
        }
        protected virtual void WriteDebug(string debugMsg)
        {
            Debug.Print(debugMsg);
        }
        protected virtual void WriteWarning(string warningMsg)
        {
            Debug.Print(warningMsg);
        }

        private void UpdateAppState(string text)
        {
            var data = text.SplitString('|');
            MsgType type;
            Enum.TryParse(data.Item1, out type);
            switch (type)
            {

                case MsgType.WindowHandle:
                    windowHandle = ParseIntPtr(data.Item2);
                    if (OnMessageWindowHwndReceived != null)
                        OnMessageWindowHwndReceived(this, new EventArgs<IntPtr>(windowHandle));
                    break;
                case MsgType.CopyData:
                    var args=data.Item2.Split(' ');
                    var copyData = new CopyData
                    {
                        Sender = ParseIntPtr(args[0]),
                        Data = long.Parse(args[1]),
                        PointerData = ParseUIntPtr(args[2]),
                        VoidData = ParseIntPtr(args[4])
                    };
                    if (OnClipboarCopyDataSent != null) 
                        OnClipboarCopyDataSent(this, new EventArgs<CopyData>(copyData));
                    break;
                case MsgType.ClipboardUpdate:
                        OnClipboardContentChanged(this, EventArgs.Empty);
                    break;
                case MsgType.DestroyClipboard:
                    if (OnClipboardContentDestroy != null) 
                        OnClipboardContentDestroy(this,EventArgs.Empty);
                    break;
                case MsgType.RenderFormat:
                    var format = Int32.Parse(data.Item2);
                    if (OnRenderFormatRequested != null) 
                        OnRenderFormatRequested(this, new EventArgs<int>(format));
                    break;
            }
        }



        private IntPtr ParseIntPtr(string ptrString)
        {
            if(string.IsNullOrWhiteSpace(ptrString))
                return IntPtr.Zero;
            var ptr = Int32.Parse(ptrString);
            return new IntPtr(ptr);
        }

        private UIntPtr ParseUIntPtr(string ptrString)
        {
            if (string.IsNullOrWhiteSpace(ptrString))
                return UIntPtr.Zero;
            var ptr = UInt32.Parse(ptrString);
            return new UIntPtr(ptr);
        }

        public IEnumerable<uint> WaitClipboardData()
        {
            while (!disposed)
            {
                WaitHandle.WaitAny(new WaitHandle[] {waitHandle});
                yield return SequenceNumber;
            }
        }

        private void ClipboardContentChanged(object sender, EventArgs eventArgs)
        {
            if(waitHandle!=null)waitHandle.Set();
        }

        public static uint SequenceNumber
        {
            get { return GetClipboardSequenceNumber(); }
        }

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        public void Stop()
        {
            if (proc == null)
            {
                proc = Process.GetProcesses(WatcherProcessName).FirstOrDefault();
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
                proc.CloseMainWindow();
            }

        }
    }
}
