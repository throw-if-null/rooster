using Microsoft.Extensions.Options;
using Rooster.MongoDb.Connectors.Databases;

namespace Rooster.MongoDb.Connectors.Colections
{
    public interface ILogbookCollectionFactory : ICollectionFactory
    {
    }

    public class LogbookCollectionFactory :
        CollectionFactory<LogbookCollectionFactoryOptions>,
        ILogbookCollectionFactory
    {
        public LogbookCollectionFactory(IOptions<LogbookCollectionFactoryOptions> options, IDatabaseFactory databaseFactory)
            : base(options, databaseFactory)
        {
        }
    }
}