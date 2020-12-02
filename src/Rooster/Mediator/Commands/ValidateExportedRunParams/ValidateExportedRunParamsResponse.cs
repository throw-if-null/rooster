namespace Rooster.Mediator.Commands.ValidateExportedRunParams
{
    public sealed record ValidateExportedRunParamsResponse
    {
        public bool IsValid { get; init; }
    }
}
