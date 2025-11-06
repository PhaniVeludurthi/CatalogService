using CatalogService.Models;

namespace CatalogService.Repository.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<IEnumerable<Event>> GetByVenueIdAsync(int venueId);
        Task<IEnumerable<Event>> GetByStatusAsync(string status);
        Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Event> CreateAsync(Event eventEntity);
        Task<Event?> UpdateAsync(Event eventEntity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Event>> GetByCityAsync(string city);
        Task<IEnumerable<Event>> SearchAsync(string query);
    }
}
