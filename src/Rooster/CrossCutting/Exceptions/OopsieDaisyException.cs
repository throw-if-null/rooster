using System;

namespace Rooster.CrossCutting.Exceptions
{
    internal sealed class OopsieDaisyException : Exception
    {
        private const string ErrorMessage = "Unhandled exception happened. It's official: We are dead.";

        private OopsieDaisyException()
        {
        }

        public OopsieDaisyException(Exception innerException) : base(ErrorMessage, innerException)
        {
        }
    }
}
