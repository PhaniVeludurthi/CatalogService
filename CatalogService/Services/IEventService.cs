namespace CatalogService.Services
{
    public interface IEventService
    {
        Task CancelEventAsync(int eventId);
    }
}
