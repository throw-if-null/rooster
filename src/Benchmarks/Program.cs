using BenchmarkDotNet.Running;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<KuduApiAdapter.KuduApiAdapterBenchmarks>();
        }
    }
}