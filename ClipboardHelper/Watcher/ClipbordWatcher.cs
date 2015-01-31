using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Utils;

namespace ClipboardHelper.Watcher
{
    public struct CopyData
    {
        public IntPtr Sender;
        public UIntPtr PointerData;
        public long Data;
        public IntPtr VoidData;

    }

    public interface IClipbordMessageProvider
    {
        IntPtr WindowHandle{get;}
    }

    public interface IClipbordWatcher
    {
        event EventHandler<EventArgs<uint>> OnClipboardContentChanged;
        event EventHandler<EventArgs> OnClipboardContentDestroy;
        event EventHandler<EventArgs> OnRenderAllFormatsRequested;
        event EventHandler<EventArgs<int>> OnRenderFormatRequested;
        event EventHandler<EventArgs<CopyData>> OnClipboarCopyDataSent;
        event EventHandler<EventArgs<IntPtr>> OnMessageWindowHwndReceived;

        void StartListen(bool throwIfAnotherListenerStarted = false);
        void Stop();

        IEnumerable<uint> WaitClipboardData();

        bool IsListenerStarted { get;}
    }

    public class ClipbordWatcher : IDisposable, IClipbordMessageProvider, IClipbordWatcher
    {
        private const string WatcherProcessName = "ClipboardWatcher.exe";
        public event EventHandler<EventArgs<uint>> OnClipboardContentChanged;
        public event EventHandler<EventArgs> OnClipboardContentDestroy;
        public event EventHandler<EventArgs> OnRenderAllFormatsRequested;
        public event EventHandler<EventArgs<int>> OnRenderFormatRequested;
        public event EventHandler<EventArgs<CopyData>> OnClipboarCopyDataSent;
        public event EventHandler<EventArgs<IntPtr>> OnMessageWindowHwndReceived;

        private bool startEventSignaled = false;
        private IntPtr windowHandle;
        private AutoResetEvent waitHandle;
        private Timer timer;
        
        private Process proc;
        private uint clipboardDataSequenceNumber;
        private Thread watcherThread=null;

        public ClipbordWatcher()
        {
            OnClipboardContentChanged += ClipboardContentChanged;
            waitHandle = new AutoResetEvent(false);
        }

        public static bool WatcherAllreadyRan()
        {
            return Process.GetProcessesByName(WatcherProcessName).Any();
        }


        public void StartListen(bool throwIfAnotherListenerStarted = false)
        {
            var startResult = new AsyncResult();
            if (proc != null) throw new ClipbordWatcherException("Cannot start second listener istance");

            if (throwIfAnotherListenerStarted && WatcherAllreadyRan())
                throw new ClipbordWatcherException(
                    "Other instance of the Clipboard watcher already started. Close another one first");
            
            watcherThread = new Thread(() =>
            {
                StartMessageWindow();
                
                StartListenLoop(proc.StandardOutput, startResult);
            });
            watcherThread.SetApartmentState(ApartmentState.STA);
            watcherThread.Start();
            if (startResult.IsCompleted)
                startResult.CompletedSynchronously = true;

            startResult.AsyncWaitHandle.WaitOne();
            StartListenerWatchDog();
        }

        #region Process WatchDog notifier
        private void StartListenerWatchDog()
        {
            timer = new Timer(TimerTick, null, 1000, 1000);
        }

        private void TimerTick(object state)
        {
            SendMessage(windowHandle, WM_PING, IntPtr.Zero, IntPtr.Zero);
        }
        #endregion


        private void StartMessageWindow()
        {

            var procStartInfo = new ProcessStartInfo(WatcherProcessName)
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                Arguments = CreateClassName()
            };
            //procStartInfo.CreateNoWindow = true;

            proc = Process.Start(procStartInfo);
        }

        protected void StartListenLoop(StreamReader standardOutput, AsyncResult startedEvent){
            string line;
            while ((line = standardOutput.ReadLine()) != null)
            {

                Tuple<string, string> data = line.SplitString(':');
                HandleListenMessage(data.Item1, data.Item2);
                if (!startedEvent.IsCompleted) startedEvent.Complete();

            }
        }

        #region Clipboard watcher process messages handling
        
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
                case MsgSeverity.PostData:
                    UpdateAppState(data);
                    break;
                case MsgSeverity.SendData:
                    OnDataSent(data);
                    break;
                case MsgSeverity.Debug:
                    WriteDebug(data);
                    break;
            }
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
                        var sequenceNumber= UInt32.Parse(data.Item2);
                        OnClipboardContentChanged(this, new EventArgs<uint>(sequenceNumber));
                    break;
                case MsgType.DestroyClipboard:
                    if (OnClipboardContentDestroy != null) 
                        OnClipboardContentDestroy(this,EventArgs.Empty);
                    break;
            }
        }

        private void OnDataSent(string text)
        {
            var data = text.SplitString('|');
            MsgType type;
            Enum.TryParse(data.Item1, out type);
            switch (type)
            {
                case MsgType.RenderFormat:
                    var format = Int32.Parse(data.Item2);
                    if (OnRenderFormatRequested != null) 
                        OnRenderFormatRequested(this, new EventArgs<int>(format));
                    break;
                case MsgType.RenderAllFormats:
                    if (OnRenderAllFormatsRequested != null)
                        OnRenderAllFormatsRequested(this, EventArgs.Empty);
                    break;
            }
            proc.StandardInput.WriteLine(data.Item1);
        }

        #endregion


        public IEnumerable<uint> WaitClipboardData()
        {
            while (!disposed)
            {
                WaitHandle.WaitAny(new WaitHandle[] {waitHandle});
                yield return clipboardDataSequenceNumber;
            }
        }

        private void ClipboardContentChanged(object sender, EventArgs<uint> eventArgs)
        {
            clipboardDataSequenceNumber = eventArgs.Value;
            if (waitHandle != null && !disposed) waitHandle.Set();
            
        }


        public bool IsListenerStarted { get { return proc != null; } }

        IntPtr IClipbordMessageProvider.WindowHandle
        {
            get { return windowHandle; }
        }

        public static uint SequenceNumber
        {
            get { return GetClipboardSequenceNumber(); }
        }

        #region Distructor
        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ClipbordWatcher()
        {
            Dispose(false);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)return;
            disposed = true;
            waitHandle.Set();
            waitHandle.Dispose();
            if(timer!=null) timer.Dispose();
            if (watcherThread != null)
            {
                SendMessage(windowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                proc.Refresh();
                if (!proc.HasExited)
                {
                    proc.WaitForExit(10);
                    proc.Refresh();
                    if(!proc.HasExited)
                        proc.Kill();
                }
                watcherThread.Abort();
            }

        }
        #endregion
        protected virtual string CreateClassName()
        {
            var classNameBuilder = new StringBuilder(AppDomain.CurrentDomain.FriendlyName);
            for (int i = 0; i < classNameBuilder.Length; i++)
            {
                char c = classNameBuilder[i];
                if (!Char.IsLetterOrDigit(c))
                    classNameBuilder[i] = '_';
            }
            return classNameBuilder.ToString();
        }
        #region Mesages loggin
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
        #endregion
        #region Helpers
        protected virtual void ThrowError(string errorMessage)
        {
            var errData = errorMessage.SplitString(':');
            var errCodeStr = errData.Item1.Replace("ErrCode", "");
            var errCode = Int32.Parse(errCodeStr);
            Marshal.ThrowExceptionForHR(errCode);
        }

        private IntPtr ParseIntPtr(string ptrString)
        {
            if (string.IsNullOrWhiteSpace(ptrString))
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



        private const int WM_CLOSE=0x0010;
        private const int WM_PING=0x0420;


        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        #endregion

    }
}
