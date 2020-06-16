using MongoDB.Bson.Serialization.Attributes;

namespace Rooster.DataAccess.KuduInstances.Entities
{
    public class KuduInstance<T>
    {
        [BsonId]
        public T Id { get; set; }

        public string Name { get; set; }
    }
}