using Rooster.CrossCutting.Exceptions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rooster.CrossCutting
{
    public readonly struct Engine
    {
        private static class Constants
        {
            internal const string Unsupported = "UNSUPPORTED";
            internal const string MongoDb = "MONGODB";
            internal const string SqlServer = "SQLSERVER";
            internal const string Slack = "SLACK";
            internal const string AppInsights = "APPINSIGHTS";
            internal const string Mock = "MOCK";
        }

        public string Name { get; }

        private Engine(string name)
        {
            Name = name;
        }

        public static Engine FromString([NotNull] string engineName)
        {
            engineName = engineName.ToUpperInvariant().Trim();

            return engineName switch
            {
                Constants.MongoDb => MongoDb,
                Constants.SqlServer => SqlServer,
                Constants.Slack => Slack,
                Constants.AppInsights => AppInsights,
                Constants.Mock => Mock,
                _ => throw new NotSupportedEngineException(engineName),
            };
        }

        public static IEnumerable<Engine> ToList(IEnumerable<string> values)
        {
            return values.Select(FromString);
        }

        public static readonly Engine Unsupported = new Engine(Constants.Unsupported);
        public static readonly Engine MongoDb = new Engine(Constants.MongoDb);
        public static readonly Engine SqlServer = new Engine(Constants.SqlServer);
        public static readonly Engine Slack = new Engine(Constants.Slack);
        public static readonly Engine AppInsights = new Engine(Constants.AppInsights);
        public static readonly Engine Mock = new Engine(Constants.Mock);

        public override bool Equals(object obj)
        {
            var engine = (Engine)obj;

            if (engine.Name.Equals(Name))
                return true;

            return false;
        }

        public static string Values => $"{MongoDb.Name}, {SqlServer.Name}, {Slack.Name}, {AppInsights.Name} and {Mock.Name}";
    }
}