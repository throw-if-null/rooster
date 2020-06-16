using Microsoft.Extensions.Options;
using Rooster.MongoDb.Connectors.Databases;

namespace Rooster.MongoDb.Connectors.Colections
{
    public interface IKuduInstanceCollectionFactory : ICollectionFactory
    {
    }

    public class KuduInstanceCollectionFactory :
        CollectionFactory<KuduInstanceCollectionFactoryOptions>,
        IKuduInstanceCollectionFactory
    {
        public KuduInstanceCollectionFactory(IOptions<KuduInstanceCollectionFactoryOptions> options, IDatabaseFactory databaseFactory)
            : base(options, databaseFactory)
        {
        }
    }
}