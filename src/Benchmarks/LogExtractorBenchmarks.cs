using BenchmarkDotNet.Attributes;
using Benchmarks;
using Rooster.CrossCutting.Docker;

namespace Rooster.Benchmarkss
{
    [MemoryDiagnoser]
    public class LogExtractorBenchmarks
    {
        public const string Data = "2020-11-11T13:08:10.906Z INFO  - docker run -d -p 8590:8081 --name bf-studioapi-sandbox_21_752839fa_msiProxy -e DOCKER_CUSTOM_IMAGE_NAME=bannerflow.azurecr.io/studio/studio-api:sandbox-26961 -e WEBSITE_SITE_NAME=bf-studioapi-sandbox -e WEBSITE_AUTH_ENABLED=False -e WEBSITE_ROLE_INSTANCE_ID=0 -e WEBSITE_HOSTNAME=bf-studioapi-sandbox.azurewebsites.net -e WEBSITE_INSTANCE_ID=fe93d7555140e730a57adee08819d151ef6f2e9107dc113c27ee40a9c8ea4fb8 -e HTTP_LOGGING_ENABLED=1 appsvc/msitokenservice:2007200210";

        [Benchmark]
        public void Extract()
        {
            var extractor = new LogExtractorOld();

            _ = extractor.ExtractContainerName(Data);
            _ = extractor.ExtractDate(Data);
            _ = extractor.ExtractImageName(Data);
            _ = extractor.ExtractPorts(Data);
            _ = extractor.ExtractServiceName(Data);
        }

        [Benchmark]
        public void Extract2()
        {
            _ = LogExtractor.Extract(Data);
        }
    }
}
