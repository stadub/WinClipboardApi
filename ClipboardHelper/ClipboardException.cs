using System;
using System.Runtime.Serialization;

namespace ClipboardHelper
{
    [Serializable]
    public class ClipboardException : Exception
    {
        public ClipboardException() { }

        public ClipboardException(string message) : base(message) { }

        public ClipboardException(string message, Exception inner) : base(message, inner) { }

        protected ClipboardException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class OpenClipboardException : ClipboardException
    {
        public OpenClipboardException(){}

        public OpenClipboardException(string message) : base(message){}

        public OpenClipboardException(Exception inner) : base("Error opening Clipboard", inner){}

        protected OpenClipboardException(SerializationInfo info,StreamingContext context) : base(info, context){}
    }

    [Serializable]
    public class ClipboardOpenedException : OpenClipboardException
    {
        public ClipboardOpenedException(){}

        public ClipboardOpenedException(string message) : base(message){}
        
        protected ClipboardOpenedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class ClipboardClosedException : OpenClipboardException
    {
        public ClipboardClosedException(){}

        public ClipboardClosedException(string message) : base(message){}

        protected ClipboardClosedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ClipboardDataException : ClipboardException
    {
        public ClipboardDataException() { }

        public ClipboardDataException(string message) : base(message) { }
        public ClipboardDataException(string message, Exception inner) : base(message, inner) { }

        protected ClipboardDataException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
}
