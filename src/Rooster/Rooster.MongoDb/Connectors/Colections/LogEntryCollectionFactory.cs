using Microsoft.Extensions.Options;
using Rooster.MongoDb.Connectors.Databases;

namespace Rooster.MongoDb.Connectors.Colections
{
    public interface ILogEntryCollectionFactory : ICollectionFactory
    {
    }

    public class LogEntryCollectionFactory :
        CollectionFactory<LogEntryCollectionFactoryOptions>,
        ILogEntryCollectionFactory
    {
        public LogEntryCollectionFactory(IOptions<LogEntryCollectionFactoryOptions> options, IDatabaseFactory databaseFactory)
            : base(options, databaseFactory)
        {
        }
    }
}