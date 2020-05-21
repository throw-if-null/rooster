using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rooster
{
    public interface ILogExtractor
    {
        DockerLogReference Extract(string line);
    }

    public class LogExtractor : ILogExtractor
    {
        public DockerLogReference Extract(string line)
        {
            var portIndex = line.IndexOf("-p");
            var portsLine = line.Substring(portIndex + 2);
            portsLine = portsLine.Remove(portsLine.IndexOf(" -"));
            var ports = portsLine.Split(":").Select(x => x.Trim()).ToArray();

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

            return new DockerLogReference
            {
                AppServiceName = website,
                ContainerName = name,
                HostName = host,
                ImageName = image,
                InboundPort = ports[0],
                OutbouondPort = ports[1],
                Date = DateTimeOffset.Parse(date)
            };
        }
    }
}
