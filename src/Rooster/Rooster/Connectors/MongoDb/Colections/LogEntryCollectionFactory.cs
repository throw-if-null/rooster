using Microsoft.Extensions.Options;
using Rooster.Connectors.MongoDb.Databases;

namespace Rooster.Connectors.MongoDb.Colections
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