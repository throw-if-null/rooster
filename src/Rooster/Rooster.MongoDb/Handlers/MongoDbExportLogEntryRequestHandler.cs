using MongoDB.Bson;
using Rooster.CrossCutting;
using Rooster.Mediator.Handlers;

namespace Rooster.MongoDb.Handlers
{
    public class MongoDbExportLogEntryRequestHandler : ExportLogEntryRequestHandler<ObjectId>
    {
        public MongoDbExportLogEntryRequestHandler(ILogExtractor extractor) : base(extractor)
        {
        }
    }
}