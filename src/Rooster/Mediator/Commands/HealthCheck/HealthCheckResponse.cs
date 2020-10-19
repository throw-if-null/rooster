namespace Rooster.Mediator.Commands.HealthCheck
{
    public class HealthCheckResponse
    {
        public string Name { get; set; }

        public string Message { get; set; }

        public bool IsHaelthy { get; set; }
    }
}
