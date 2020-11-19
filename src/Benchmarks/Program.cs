using BenchmarkDotNet.Running;
using Benchmarks.WebHookReporter;
using System.Threading.Tasks;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<KuduApiAdapter.KuduApiAdapterBenchmarks>();
        }
    }
}