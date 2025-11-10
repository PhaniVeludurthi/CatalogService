
using CatalogService.Models;
using CatalogService.Repository.Interfaces;

namespace CatalogService.Services
{
    public class EventService(IEventRepository eventRepository, HttpClient httpClient, ILogger<EventService> logger, IConfiguration configuration, ICorrelationService correlationService) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<EventService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly ICorrelationService _correlationService = correlationService;
        public async Task CancelEventAsync(int eventId)
        {
            // Update event status in database
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event {eventId} not found");

            eventEntity.Status = "CANCELLED";
            eventEntity.CancelledAt = DateTime.UtcNow;
            await _eventRepository.UpdateAsync(eventEntity);

            _logger.LogInformation("Event cancelled: EventId={EventId}", eventId);

            // Notify Order Service via webhook (fire-and-forget for simplicity)
            await NotifyOrderServiceAsync(eventEntity);
        }
        private async Task NotifyOrderServiceAsync(Event eventEntity)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                var webhook = new
                {
                    eventId = eventEntity.EventId,
                    eventTitle = eventEntity.Title,
                    cancelledAt = eventEntity.CancelledAt,
                    reason = "Event cancelled by organizer"
                };

                var orderServiceUrl = _configuration["Services:OrderServiceUrl"];
                _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                    "X-Correlation-ID", _correlationService.GetCorrelationId());

                var response = await _httpClient.PostAsJsonAsync(
                    $"{orderServiceUrl}/api/webhooks/event-cancelled",
                    webhook, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order Service notified successfully: EventId={EventId}", eventEntity.EventId);
                }
                else
                {
                    _logger.LogWarning("Failed to notify Order Service: EventId={EventId}, StatusCode={StatusCode}", eventEntity.EventId, response.StatusCode);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Notification to Order Service timed out for EventId={EventId}", eventEntity.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while notifying Order Service: EventId={EventId}", eventEntity.EventId);
            }
        }

    }
}
