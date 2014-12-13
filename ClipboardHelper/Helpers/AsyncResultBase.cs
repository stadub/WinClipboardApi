using System;
using System.Threading;

namespace ClipboardHelper.Helpers
{

    public class AsyncResult : IAsyncResult
    {
        private readonly ManualResetEvent asyncWaitHandle;

        public AsyncResult()
        {
            asyncWaitHandle = new ManualResetEvent(false);
        }

        public bool IsCompleted { get; private set; }

        public void Complete()
        {
            IsCompleted = true;
            asyncWaitHandle.Set();
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return asyncWaitHandle; }
        }

        public virtual object AsyncState
        {
            get { return null; }
        }

        public bool CompletedSynchronously { get; set; }
    }
}
