using CatalogService.Context;
using CatalogService.Models;
using CatalogService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository
{
    public class VenueRepository(CatalogDbContext context, ILogger<VenueRepository> logger) : IVenueRepository
    {
        private readonly CatalogDbContext _context = context;
        private readonly ILogger<VenueRepository> _logger = logger;

        public async Task<Venue?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Venues
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.VenueId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving venue with ID {VenueId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Venue>> GetAllAsync()
        {
            try
            {
                return await _context.Venues
                    .AsNoTracking()
                    .OrderBy(v => v.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all venues");
                throw;
            }
        }

        public async Task<IEnumerable<Venue>> GetByCityAsync(string city)
        {
            try
            {
                return await _context.Venues
                    .AsNoTracking()
                    .Where(v => v.City == city)
                    .OrderBy(v => v.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving venues for city {City}", city);
                throw;
            }
        }

        public async Task<Venue> CreateAsync(Venue venue)
        {
            try
            {
                _context.Venues.Add(venue);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created venue with ID {VenueId}", venue.VenueId);
                return venue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating venue");
                throw;
            }
        }

        public async Task<Venue?> UpdateAsync(Venue venue)
        {
            try
            {
                var existing = await _context.Venues.FindAsync(venue.VenueId);
                if (existing == null)
                {
                    _logger.LogWarning("Venue with ID {VenueId} not found for update", venue.VenueId);
                    return null;
                }

                existing.Name = venue.Name;
                existing.City = venue.City;
                existing.Capacity = venue.Capacity;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated venue with ID {VenueId}", venue.VenueId);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating venue with ID {VenueId}", venue.VenueId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var venue = await _context.Venues.FindAsync(id);
                if (venue == null)
                {
                    _logger.LogWarning("Venue with ID {VenueId} not found for deletion", id);
                    return false;
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted venue with ID {VenueId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting venue with ID {VenueId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Venues.AnyAsync(v => v.VenueId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if venue exists with ID {VenueId}", id);
                throw;
            }
        }
    }
}
