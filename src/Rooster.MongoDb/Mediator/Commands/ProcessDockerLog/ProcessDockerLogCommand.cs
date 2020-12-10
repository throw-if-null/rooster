using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Commands.ProcessDockerLog
{
    public class MongoDbProcessDockerLogCommand : ProcessDockerLogCommand
    {
        public MongoDbProcessDockerLogCommand(IMediator mediator) : base(mediator)
        {
        }

        protected override async Task<bool> ShouldProcessDockerLog(DockerRunParams parameters, CancellationToken cancellationToken)
        {
            var latestLogEntry =
                await
                    Mediator.Send(
                        new GetLatestByServiceAndContainerNamesRequest
                        {
                            ContainerName = parameters.ContainerName,
                            ServiceName = parameters.ServiceName
                        },
                        cancellationToken);

            return parameters.EventDate > latestLogEntry;
        }
    }
}
