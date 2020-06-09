using Microsoft.Extensions.Options;
using Rooster.Connectors.MongoDb.Databases;

namespace Rooster.Connectors.MongoDb.Colections
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