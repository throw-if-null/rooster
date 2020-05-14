using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Rooster
{
    class Program
    {
        private const string User = "$bf-studioapi-sandbox";
        private const string Password = "vuicJaYWb9lzK1snu0mD1my8SJ6mkAofemR4LwtCcwmMqYqTG6Cplwkd28qH";
        private const string LogsPath = @"https://bf-studioapi-sandbox.scm.azurewebsites.net/api/logs/docker";

        internal static async Task Main(string[] args)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{User}:{Password}")));

            var response = await client.GetAsync(LogsPath);

            response.EnsureSuccessStatusCode();

            Stream stream = null;
            try
            {
                stream = await response.Content.ReadAsStreamAsync();
                if (!stream.CanRead)
                    return;

                using var reader = new StreamReader(stream);
                stream = null;

                var content = await reader.ReadToEndAsync();

                var logs = JsonConvert.DeserializeObject<List<LogReference>>(content);

                foreach (var log in logs)
                {
                    stream = await client.GetStreamAsync(log.Href);

                    using var logReader = new StreamReader(stream);
                    stream = null;

                    var line = "x";
                    while (line != null)
                    {
                        line = await logReader.ReadLineAsync();

                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        if (line.Contains("docker", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine(line);

                            var portIndex = line.IndexOf("-p");
                            var portsLine = line.Substring(portIndex + 2);
                            portsLine = portsLine.Remove(portsLine.IndexOf(" -"));
                            var ports = portsLine.Split(":").Select(x => x.Trim());

                            var imageIndex = line.IndexOf("DOCKER_CUSTOM_IMAGE_NAME");
                            var imageLine = line.Substring(imageIndex + 25);
                            var image = imageLine.Remove(imageLine.IndexOf(" -e"));

                            var websiteIndex = line.IndexOf("WEBSITE_SITE_NAME");
                            var website = line.Substring(websiteIndex + 18);
                            website = website.Remove(website.IndexOf(" -e"));

                            var hostIndex = line.IndexOf("WEBSITE_HOSTNAME");
                            var host = line.Substring(hostIndex + 17);
                            host = host.Remove(host.IndexOf(" -e"));

                            var date = line.Remove(line.IndexOf("INFO") - 1);

                            var nameIndex = line.IndexOf("--name");
                            var name = line.Substring(nameIndex + 7);
                            name = name.Remove(name.IndexOf(" -e"));
                        }
                    }
                }
            }
            finally
            {
                if (stream != null)
                    await stream.DisposeAsync();
            }

            Console.ReadKey();

            //2020-05-13T14:29:31.118Z INFO
            // - docker run
            // -d
            // -p 2170:80 --name bf-studioapi-sandbox_0_cbc73996
            // -e DOCKER_CUSTOM_IMAGE_NAME=bannerflow.azurecr.io/studio/studio-api:sandbox-21002
            // -e WEBSITE_SITE_NAME=bf-studioapi-sandbox
            // -e WEBSITE_AUTH_ENABLED=False
            // -e PORT=80
            // -e WEBSITE_ROLE_INSTANCE_ID=0
            // -e WEBSITE_HOSTNAME=bf-studioapi-sandbox.azurewebsites.net
            // -e WEBSITE_INSTANCE_ID=b37733b1478f260fde7f917b4a8fac623ddee02f59c646954431d7c59e948a81
            // -e HTTP_LOGGING_ENABLED=1 bannerflow.azurecr.io/studio/studio-api:sandbox-21002

        }
    }
}
