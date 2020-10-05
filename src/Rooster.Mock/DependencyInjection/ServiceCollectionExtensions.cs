using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mock.Commands;
using Rooster.Mock.Reporters;

namespace Rooster.Mock.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMock(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IMockReporter, MockReporter>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, MockProcessLogEntryCommand>();

            return services;
        }
    }
}