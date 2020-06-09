using Microsoft.Extensions.Options;
using Rooster.Connectors.MongoDb.Databases;

namespace Rooster.Connectors.MongoDb.Colections
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
