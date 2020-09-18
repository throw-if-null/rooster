using Serilog.Core;
using Serilog.Events;
using System;

namespace Rooster.CrossCutting.Serilog
{
    public sealed class CorrelationIdEnricher : ILogEventEnricher
    {
        private const string CorrelationId = "CorrelationId";

        private readonly IInstrumentationContext _context;

        public CorrelationIdEnricher(IInstrumentationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CorrelationId, _context.CorrelationValue));
        }
    }
}