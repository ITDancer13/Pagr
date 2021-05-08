using System;

namespace Pagr.Exceptions
{
    public class PagrIncompatibleMethodException : PagrException
    {
        public string MethodName { get; }

        public Type ExpectedType { get; }

        public Type ActualType { get; }

        public PagrIncompatibleMethodException(string methodName, Type expectedType, Type actualType, string message)
            : base(message)
        {
            MethodName = methodName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public PagrIncompatibleMethodException(string methodName, Type expectedType, Type actualType, string message, Exception innerException)
            : base(message, innerException)
        {
            MethodName = methodName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public PagrIncompatibleMethodException(string message) : base(message)
        {
        }

        public PagrIncompatibleMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PagrIncompatibleMethodException()
        {
        }

        protected PagrIncompatibleMethodException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
