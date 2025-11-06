using CatalogService.Models;

namespace CatalogService.Repository.Interfaces
{
    public interface IVenueRepository
    {
        Task<Venue?> GetByIdAsync(int id);
        Task<IEnumerable<Venue>> GetAllAsync();
        Task<IEnumerable<Venue>> GetByCityAsync(string city);
        Task<Venue> CreateAsync(Venue venue);
        Task<Venue?> UpdateAsync(Venue venue);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
