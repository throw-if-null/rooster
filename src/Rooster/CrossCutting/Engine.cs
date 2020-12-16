using Rooster.CrossCutting.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rooster.CrossCutting
{
    public readonly struct Engine
    {
        private const string UnsupportedName = "UNSUPPORTED";
        private const string MongoDbName = "MONGODB";
        private const string SqlServerName = "SQLSERVER";
        private const string SlackName = "SLACK";
        private const string AppInsightsName = "APPINSIGHTS";
        private const string MockName = "MOCK";

        public static readonly Engine Unsupported = new Engine(UnsupportedName);
        public static readonly Engine MongoDb = new Engine(MongoDbName);
        public static readonly Engine SqlServer = new Engine(SqlServerName);
        public static readonly Engine Slack = new Engine(SlackName);
        public static readonly Engine AppInsights = new Engine(AppInsightsName);
        public static readonly Engine Mock = new Engine(MockName);

        public string Name { get; }

        private Engine(string name)
        {
            Name = name;
        }

        public static Engine FromName(string engineName)
        {
            if (string.IsNullOrWhiteSpace(engineName))
                engineName = UnsupportedName;

            engineName = engineName.ToUpperInvariant().Trim();

            return engineName switch
            {
                MongoDbName => MongoDb,
                SqlServerName => SqlServer,
                SlackName => Slack,
                AppInsightsName => AppInsights,
                MockName => Mock,
                _ => throw new NotSupportedEngineException(engineName),
            };
        }

        public static IEnumerable<Engine> ToList(IEnumerable<string> values)
        {
            if (values == null)
                values = Enumerable.Empty<string>();

            return values.Select(FromName);
        }

        public static string Values => $"{MongoDb.Name}, {SqlServer.Name}, {Slack.Name}, {AppInsights.Name} and {Mock.Name}";

        public static bool operator ==(Engine left, Engine right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Engine left, Engine right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            var engine = (Engine)obj;

            if (engine.Name.Equals(Name))
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}