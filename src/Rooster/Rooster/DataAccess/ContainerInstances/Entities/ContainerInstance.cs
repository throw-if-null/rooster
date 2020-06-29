using MongoDB.Bson.Serialization.Attributes;

namespace Rooster.DataAccess.ContainerInstances.Entities
{
    public class ContainerInstance<T>
    {
        [BsonId]
        public T Id { get; set; }

        public string Name { get; set; }

        public T AppServiceId { get; set; }
    }
}