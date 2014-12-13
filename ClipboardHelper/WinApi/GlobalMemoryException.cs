using System;
using System.Runtime.Serialization;

namespace ClipboardHelper.Win32
{
    [Serializable]
    public class GlobalMemoryException : Exception
    {
        public GlobalMemoryException(){}

        public GlobalMemoryException(string message) : base(message){}

        public GlobalMemoryException(string message, Exception inner) : base(message, inner) { }

        protected GlobalMemoryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class AlreadyHaveMemoryBlcokException : GlobalMemoryException
    {
        public AlreadyHaveMemoryBlcokException() { }

        public AlreadyHaveMemoryBlcokException(string message) : base(message) { }

        public AlreadyHaveMemoryBlcokException(string message, Exception inner) : base(message, inner) { }

        protected AlreadyHaveMemoryBlcokException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class NotHaveMemoryBlcokException : GlobalMemoryException
    {
        public NotHaveMemoryBlcokException() { }

        public NotHaveMemoryBlcokException(string message) : base(message) { }

        public NotHaveMemoryBlcokException(string message, Exception inner) : base(message, inner) { }

        protected NotHaveMemoryBlcokException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}