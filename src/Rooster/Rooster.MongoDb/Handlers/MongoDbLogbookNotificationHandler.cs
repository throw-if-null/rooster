using MongoDB.Bson;
using Rooster.Adapters.Kudu;
using Rooster.Handlers;
using Rooster.Services;

namespace Rooster.MongoDb.Handlers
{
    public class MongoDbLogbookNotificationHandler : LogbookNotificationHandler<ObjectId>
    {
        public MongoDbLogbookNotificationHandler(
            ILogbookService<ObjectId> logbookService,
            IAppServiceService<ObjectId> appServiceService,
            IContainerInstanceService<ObjectId> containerInstanceService,
            IKuduApiAdapter<ObjectId> kudu)
            : base(logbookService, appServiceService, containerInstanceService, kudu)
        {
        }
    }
}