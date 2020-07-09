namespace Rooster.MongoDb.Connectors.Colections
{
    public abstract class CollectionFactoryOptions
    {
        public string Name { get; set; }
    }

    public sealed class AppServiceCollectionFactoryOptions : CollectionFactoryOptions
    {
    }

    public sealed class KuduInstanceCollectionFactoryOptions : CollectionFactoryOptions
    {
    }

    public sealed class LogEntryCollectionFactoryOptions : CollectionFactoryOptions
    {
    }
}