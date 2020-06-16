using MongoDB.Bson.Serialization.Attributes;

namespace Rooster.DataAccess.AppServices.Entities
{
    public class AppService<T>
    {
        [BsonId]
        public T Id { get; set; }

        public string Name { get; set; }
    }
}