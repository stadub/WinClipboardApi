using System;
using System.Diagnostics;

namespace Utils
{
    public interface IOperationResult
    {
        bool Success { get; }
        Object Value { get; }
        Exception Error { get; }
    }

    public interface IOperationResult<out T> : IOperationResult
    {
        bool Success { get; }
        T Value { get; }
        Exception Error { get; }
    }


    [DebuggerDisplay("Success = {Success}; Value = {Value}; Error = {Error}", Type = "typeof(T)" )]
    public class OperationResult<T>:IOperationResult<T>
    {
        [DebuggerStepThrough]
        public static OperationResult<T> Successful(T value)
        {
            return new OperationResult<T> { Success = true, Value = value };
        }

        [DebuggerStepThrough]
        public static OperationResult<T> Failed(Exception exception=null)
        {
            return new OperationResult<T> { Success = false, Error = exception };
        }

        protected OperationResult(bool success,T value,Exception error = null)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        protected OperationResult()
        {
        }

        public bool Success { get; private set; }
        object IOperationResult.Value { get { return this.Value; } }
        public T Value { get; private set; }
        public Exception Error { get; private set; }
    }

    [DebuggerDisplay("Success = {Success}; Value = {Value}; Error = {Error}")]
    public class OperationResult : OperationResult<object>
    {
        protected OperationResult(bool success, object value, Exception error = null)
            : base(success, value,error){}

        [DebuggerStepThrough]
        public new static OperationResult Failed(Exception exception = null)
        {
            return new OperationResult(false,null,exception);
        }

        [DebuggerStepThrough]
        public static OperationResult Successful(object value)
        {
            return new OperationResult(true, value);
        }
    }
}
