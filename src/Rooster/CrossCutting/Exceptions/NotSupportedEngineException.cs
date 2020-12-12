using System;

namespace Rooster.CrossCutting.Exceptions
{
    public class NotSupportedEngineException : Exception
    {
        private static readonly Func<string, string> BuildErrorMessage =
            delegate (string engine)
            {
                return $"Engine: {engine} is not supported. Supported values are: {Engines.Values}.";
            };

        public NotSupportedEngineException(string databaseEngine) :
            base(BuildErrorMessage(databaseEngine))
        {
        }

        public NotSupportedEngineException(string databaseEngine, Exception innerException)
            : base(BuildErrorMessage(databaseEngine), innerException)
        {
        }
    }
}
