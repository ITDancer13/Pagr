using System;
using System.Runtime.Serialization;

namespace Pagr.Exceptions
{
    [Serializable]
    public class PagrException : Exception
    {
        public PagrException()
        {
        }

        public PagrException(string message) : base(message)
        {
        }

        public PagrException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PagrException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
