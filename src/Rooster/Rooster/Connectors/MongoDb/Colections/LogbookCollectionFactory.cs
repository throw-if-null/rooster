using Microsoft.Extensions.Options;
using Rooster.Connectors.MongoDb.Databases;

namespace Rooster.Connectors.MongoDb.Colections
{
    public interface ILogbooCollectionFactory : ICollectionFactory
    {
    }

    public class LogbookCollectionFactory :
        CollectionFactory<LogbookCollectionFactoryOptions>,
        ILogbooCollectionFactory
    {
        public LogbookCollectionFactory(IOptions<LogbookCollectionFactoryOptions> options, IDatabaseFactory databaseFactory)
            : base(options, databaseFactory)
        {
        }
    }
}