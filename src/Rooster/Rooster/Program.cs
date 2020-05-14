using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
                            Console.WriteLine(line);
                    }
                }
            }
            finally
            {
                if (stream != null)
                    await stream.DisposeAsync();
            }

            Console.ReadKey();
        }
    }
}
