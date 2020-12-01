namespace Rooster.Mediator.Commands.HealthCheck
{
    public record HealthCheckResponse
    {
        public string Name { get; init; }

        public string Message { get; init; }

        public bool IsHaelthy { get; init; }
    }
}
