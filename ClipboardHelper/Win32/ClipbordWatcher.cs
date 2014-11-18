using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClipboardHelper.Helpers;
using ClipboardHelper.Win32.ClipbordWatcherTypes;

namespace ClipboardHelper.Win32
{

  
    public class ClipbordWatcher:IDisposable
    {

        public EventHandler<EventArgs> ClipboardContentChanged;

        public ClipbordWatcher()
        {
            waitHandle= new ManualResetEvent(false);
        }

        private Process proc;
        public void Start()
        {
            //var procStartInfo = new ProcessStartInfo("ClipboardWatcher.exe");
            var procStartInfo = new ProcessStartInfo(
@"C:\Users\Dima\Documents\Visual Studio 2013\Projects\ClipbordHelper\Debug\ClipboardWatcher.exe");
            //procStartInfo.CreateNoWindow = true;
            procStartInfo.RedirectStandardOutput= true;
            procStartInfo.RedirectStandardInput= true;
            procStartInfo.UseShellExecute = false;

            var classNameBuilder= new StringBuilder(AppDomain.CurrentDomain.FriendlyName);
            for (int i = 0; i < classNameBuilder.Length; i++)
            {
                char c = classNameBuilder[i];
                if (!Char.IsLetterOrDigit(c))
                    classNameBuilder[i] = '_';
            }

            procStartInfo.Arguments = classNameBuilder.ToString();
            proc=Process.Start(procStartInfo);

            string line;
            while ((line=proc.StandardOutput.ReadLine())!=null)
            {

                var data = line.SplitString(':');

                MsgSeverity severity;
                MsgSeverity.TryParse(data.Item1, out severity);

                switch (severity)
                {
                    case MsgSeverity.Error:
                        var err = data.Item2;
                        var errData=err.SplitString(':');
                        var errCodeStr = errData.Item1.Replace("ErrCode", "");
                        var errCode = Int32.Parse(errCodeStr);
                        Marshal.ThrowExceptionForHR(errCode);
                        break;
                    case MsgSeverity.Warning:
                        break;
                    case MsgSeverity.Info:
                        break;
                    case MsgSeverity.AppData:
                        UpdateAppState(data.Item2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               
            }
        }


        private void UpdateAppState(string text)
        {
            MsgType type;
            MsgType.TryParse(data.Item2, out type);
            switch (type)
            {

                case MsgType.WindowHandle:
                    ParseHwnd(args[1]);
                    //var handle = new UIntPtr(hwndPtr);
                    break;
                case MsgType.CopyData:
                    break;
                case MsgType.ClipboardUpdate:
                    if (ClipboardContentChanged != null) ClipboardContentChanged(this, new EventArgs());
                    break;
                case MsgType.DestroyClipboard:
                    break;
                case MsgType.RenderFormat:
                    break;
            }
        }

        private IntPtr ParseHwnd(string hwndString)
        {
            var hwndPtr = Int32.Parse(hwndString,NumberStyles.AllowHexSpecifier);
            return new IntPtr(hwndPtr);
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

        private void WaitClipboardContentChange()
        {
            waitHandle.Set();
        }

        private Task watcherTask;
        public void Create()
        {
            

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
