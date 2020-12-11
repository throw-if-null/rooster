using System;
using System.Runtime.Serialization;

namespace Rooster.CrossCutting.Exceptions
{
    /// <summary>
    /// Exception used to break the polling loop when internal poller is used.
    /// </summary>
    public class PollingCanceledException : Exception
    {
        public PollingCanceledException() : base("Polling canceled")
        {
        }

        public PollingCanceledException(string message) : base(message)
        {
        }

        public PollingCanceledException(Exception innerException) : base("Polling canceled", innerException)
        {
        }

        public PollingCanceledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PollingCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
