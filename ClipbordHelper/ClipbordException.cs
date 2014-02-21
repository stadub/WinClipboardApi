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

    [Serializable]
    public class OpenClipbordException : ClipbordException
    {
        public OpenClipbordException(){}

        public OpenClipbordException(string message) : base(message){}

        public OpenClipbordException(Exception inner) : base("Error opening clipbord", inner){}

        protected OpenClipbordException(SerializationInfo info,StreamingContext context) : base(info, context){}
    }
    [Serializable]
    public class ClipbordOpenedException : OpenClipbordException
    {
        public ClipbordOpenedException(){}

        public ClipbordOpenedException(string message) : base(message){}
        
        protected ClipbordOpenedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
}
