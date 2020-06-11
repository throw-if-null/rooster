using MongoDB.Bson;
using Rooster.DataAccess.KuduInstances.Entities;

namespace Rooster.DataAccess.Logbooks.Entities
{
    public class MongoDbLogbook : Logbook<ObjectId, MongoDbKuduInstance>
    {
    }
}
