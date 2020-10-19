namespace Rooster
{
    public static class Engines
    {
        public const string MongoDb = "MONGODB";

        public const string SqlServer = "SQLSERVER";

        public const string Slack = "SLACK";

        public const string AppInsights = "APPINSIGHTS";

        public const string Mock = "MOCK";

        public static string Values => $"{MongoDb}, {SqlServer}, {Slack}, {AppInsights} and {Mock}";
    }
}