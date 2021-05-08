using System;

namespace Pagr.Exceptions
{
    public class PagrMethodNotFoundException : PagrException
    {
        public string MethodName { get; }

        public PagrMethodNotFoundException(string methodName, string message) : base(message)
        {
            MethodName = methodName;
        }

        public PagrMethodNotFoundException(string methodName, string message, Exception innerException) : base(message, innerException)
        {
            MethodName = methodName;
        }

        public PagrMethodNotFoundException(string message) : base(message)
        {
        }

        public PagrMethodNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PagrMethodNotFoundException()
        {
        }

        protected PagrMethodNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
