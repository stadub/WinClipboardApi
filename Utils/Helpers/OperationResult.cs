using System;

namespace Utils
{
    public interface IOperationResult<out T>
    {
        bool Success { get; }
        T Value { get; }
        Exception Error { get; }
    }

    public class OperationResult
    {
        public static OperationResult<T> Successfull<T>(T value)
        {
            return OperationResult<T>.Successful(value);
        }

        public static OperationResult<object> Failed(Exception exception = null)
        {
            return OperationResult<object>.Failed(exception);
        }

    }

    public class OperationResult<T>:IOperationResult<T>
    {

        public static OperationResult<T> Successful(T value)
        {
            return new OperationResult<T> { Success = true, Value = value };
        }

        public static OperationResult<T> Failed(Exception exception=null)
        {
            return new OperationResult<T> { Success = false, Error = exception };
        }

        protected OperationResult()
        {
        }

        public bool Success { get; private set; }
        public T Value { get; private set; }
        public Exception Error { get; private set; }
    }
}
