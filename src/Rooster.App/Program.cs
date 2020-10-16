using Rooster.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.App
{
    internal class Program
    {
        private static readonly Func<CancellationToken> BuildCancellationToken = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static async Task Main(string[] args)
        {
            await Runner.Run(BuildCancellationToken());
        }
    }
}