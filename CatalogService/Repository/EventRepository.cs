using CatalogService.Context;
using CatalogService.Models;
using CatalogService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository
{
    public class EventRepository(CatalogDbContext context, ILogger<EventRepository> logger) : IEventRepository
    {
        private readonly CatalogDbContext _context = context;
        private readonly ILogger<EventRepository> _logger = logger;

        public async Task<Event?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.EventId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event with ID {EventId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all events");
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetByVenueIdAsync(int venueId)
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .Where(e => e.VenueId == venueId)
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events for venue {VenueId}", venueId);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetByStatusAsync(string status)
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .Where(e => e.Status == status)
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events with status {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .Where(e => e.EventDate >= startDate && e.EventDate <= endDate)
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events between {StartDate} and {EndDate}",
                    startDate, endDate);
                throw;
            }
        }

        public async Task<Event> CreateAsync(Event eventEntity)
        {
            try
            {
                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created event with ID {EventId}", eventEntity.EventId);
                return eventEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                throw;
            }
        }

        public async Task<Event?> UpdateAsync(Event eventEntity)
        {
            try
            {
                var existing = await _context.Events.FindAsync(eventEntity.EventId);
                if (existing == null)
                {
                    _logger.LogWarning("Event with ID {EventId} not found for update", eventEntity.EventId);
                    return null;
                }

                existing.VenueId = eventEntity.VenueId;
                existing.Title = eventEntity.Title;
                existing.EventType = eventEntity.EventType;
                existing.EventDate = eventEntity.EventDate;
                existing.BasePrice = eventEntity.BasePrice;
                existing.Status = eventEntity.Status;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated event with ID {EventId}", eventEntity.EventId);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event with ID {EventId}", eventEntity.EventId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var eventEntity = await _context.Events.FindAsync(id);
                if (eventEntity == null)
                {
                    _logger.LogWarning("Event with ID {EventId} not found for deletion", id);
                    return false;
                }

                _context.Events.Remove(eventEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted event with ID {EventId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event with ID {EventId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Events.AnyAsync(e => e.EventId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if event exists with ID {EventId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetByCityAsync(string city)
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .Where(e => e.Venue.City == city)
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events for city {City}", city);
                throw;
            }
        }
        public async Task<IEnumerable<Event>> SearchAsync(string query)
        {
            try
            {
                var searchTerm = query.ToLower();

                return await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .Where(e =>
                        e.Title.ToLower().Contains(searchTerm) ||
                        e.EventType.ToLower().Contains(searchTerm) ||
                        e.Venue.Name.ToLower().Contains(searchTerm) ||
                        e.Venue.City.ToLower().Contains(searchTerm))
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching events with query {Query}", query);
                throw;
            }
        }
    }
}
