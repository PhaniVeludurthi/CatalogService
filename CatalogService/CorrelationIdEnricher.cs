using CatalogService.Services;
using Serilog.Core;
using Serilog.Events;

namespace CatalogService
{
    public class CorrelationIdEnricher(ICorrelationService correlationService) : ILogEventEnricher
    {
        private readonly ICorrelationService _correlationService = correlationService;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = _correlationService.GetCorrelationId();
            var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
