using System;

namespace Rooster.DependencyInjection.Exceptions
{
    public class NotSupportedDataStoreException : Exception
    {
        private static readonly Func<string, string> BuildErrorMessage =
            delegate (string databaseEngine)
            {
                return $"Database: {databaseEngine} is not supported. Supported values are: MongoDb and SqlServer.";
            };

        public NotSupportedDataStoreException(string databaseEngine) : base(BuildErrorMessage(databaseEngine))
        {
        }

        public NotSupportedDataStoreException(string databaseEngine, Exception innerException)
            : base(BuildErrorMessage(databaseEngine), innerException)
        {
        }
    }
}
