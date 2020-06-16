using Microsoft.Extensions.Options;
using Rooster.MongoDb.Connectors.Databases;

namespace Rooster.MongoDb.Connectors.Colections
{
    public interface IAppServiceCollectionFactory : ICollectionFactory
    {
    }

    public class AppServiceCollectionFactory :
        CollectionFactory<AppServiceCollectionFactoryOptions>,
        IAppServiceCollectionFactory
    {
        public AppServiceCollectionFactory(IOptions<AppServiceCollectionFactoryOptions> options, IDatabaseFactory databaseFactory)
            : base(options, databaseFactory)
        {
        }
    }
}
