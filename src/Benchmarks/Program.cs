using BenchmarkDotNet.Running;
using Rooster.Benchmarkss;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<LogExtractorBenchmarks>();
        }
    }
}