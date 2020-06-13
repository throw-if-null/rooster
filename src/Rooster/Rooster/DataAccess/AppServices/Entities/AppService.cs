using MongoDB.Bson.Serialization.Attributes;

namespace Rooster.DataAccess.AppServices.Entities
{
    public interface IAppService
    {
    }

    public abstract class AppService<T> : IAppService
    {
        [BsonId]
        public T Id { get; set; }
        public string Name { get; set; }
    }
}