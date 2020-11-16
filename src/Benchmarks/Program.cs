using BenchmarkDotNet.Running;
using Benchmarks.KuduApiAdapter;
using System.Threading.Tasks;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<KuduApiAdapterBenchmarks>();
        }
    }
}