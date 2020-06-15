using Microsoft.Extensions.Options;
using Rooster.Connectors.MongoDb.Databases;

namespace Rooster.Connectors.MongoDb.Colections
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