using System;
using System.Runtime.Serialization;

namespace ClipboardHelper.Watcher
{
    [Serializable]
    public class ClipbordWatcherException : Exception
    {
        public ClipbordWatcherException() { }

        public ClipbordWatcherException(string message) : base(message) { }

        public ClipbordWatcherException(string message, Exception inner) : base(message, inner) { }

        protected ClipbordWatcherException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
