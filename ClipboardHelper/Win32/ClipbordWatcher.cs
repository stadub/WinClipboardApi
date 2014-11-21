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
                        ThrowError(data.Item2);
                        break;
                    case MsgSeverity.Warning:
                        WriteWarning(data.Item2);
                        break;
                    case MsgSeverity.Info:
                        WriteInfo(data.Item2);
                        break;
                    case MsgSeverity.AppData:
                        UpdateAppState(data.Item2);
                        break;
                    case MsgSeverity.Debug:
                        WriteDebug(data.Item2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               
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
            MsgType.TryParse(data.Item2, out type);
            switch (type)
            {

                case MsgType.WindowHandle:

                    var handle = ParseHwnd(text);
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
