using System;
using System.Runtime.Serialization;

namespace ClipbordHelper
{
    [Serializable]
    public class ClipbordException : Exception
    {
        public ClipbordException() { }

        public ClipbordException(string message) : base(message) { }

        public ClipbordException(string message, Exception inner) : base(message, inner) { }

        protected ClipbordException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
