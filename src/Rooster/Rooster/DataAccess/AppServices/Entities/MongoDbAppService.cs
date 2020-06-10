using MongoDB.Bson;

namespace Rooster.DataAccess.AppServices.Entities
{
    public class MongoDbAppService : IAppService<ObjectId>
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}