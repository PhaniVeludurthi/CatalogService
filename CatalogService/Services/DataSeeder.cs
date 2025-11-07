using CatalogService.Context;
using CatalogService.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CatalogService.Services
{
    public class DatabaseSeeder(CatalogDbContext context, ILogger<DatabaseSeeder> logger)
    {
        private readonly CatalogDbContext _context = context;
        private readonly ILogger<DatabaseSeeder> _logger = logger;

        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting database seeding...");

            try
            {
                // Ensure database is created and migrations applied
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied");

                var venueCount = await _context.Venues.CountAsync();
                if (venueCount == 0)
                {
                    // Seed venues
                    await SeedVenuesAsync();
                }
                else
                {
                    _logger.LogInformation("venues table already has {Count} records, skipping seed", venueCount);
                }

                var eventCount = await _context.Events.CountAsync();
                if (eventCount == 0)
                {
                    // Seed events
                    await SeedEventsAsync();
                }
                else
                {
                    _logger.LogInformation("events table already has {Count} records, skipping seed", eventCount);
                }


                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }

        private async Task SeedVenuesAsync()
        {
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "etsr_venues.csv");

            if (!File.Exists(csvPath))
            {
                _logger.LogWarning("Venues CSV file not found at: {Path}", csvPath);
                return;
            }

            _logger.LogInformation("Seeding venues from CSV: {Path}", csvPath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<VenueCsvMap>();
            var records = csv.GetRecords<VenueCsv>().ToList();

            _logger.LogInformation("Found {Count} venues in CSV", records.Count);

            var venues = records.Select(r => new Venue
            {
                VenueId = r.VenueId,
                Name = r.Name,
                City = r.City,
                Capacity = r.Capacity
            }).ToList();

            await _context.Venues.AddRangeAsync(venues);
            await _context.SaveChangesAsync();

            // Reset identity sequence
            await ResetVenueSequenceAsync();

            _logger.LogInformation("Successfully seeded {Count} venues", venues.Count);
        }

        private async Task SeedEventsAsync()
        {
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "etsr_events.csv");

            if (!File.Exists(csvPath))
            {
                _logger.LogWarning("Events CSV file not found at: {Path}", csvPath);
                return;
            }

            _logger.LogInformation("Seeding events from CSV: {Path}", csvPath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<EventCsvMap>();
            var records = csv.GetRecords<EventCsv>().ToList();

            _logger.LogInformation("Found {Count} events in CSV", records.Count);

            var events = records.Select(r => new Event
            {
                EventId = r.EventId,
                VenueId = r.VenueId,
                Title = r.Title,
                EventType = r.EventType,
                EventDate = DateTime.SpecifyKind(r.EventDate, DateTimeKind.Utc),
                BasePrice = r.BasePrice,
                Status = r.Status
            }).ToList();

            // Add in batches to avoid memory issues
            const int batchSize = 100;
            for (int i = 0; i < events.Count; i += batchSize)
            {
                var batch = events.Skip(i).Take(batchSize).ToList();
                await _context.Events.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Seeded batch {Batch}/{Total} events",
                    Math.Min(i + batchSize, events.Count), events.Count);
            }

            // Reset identity sequence
            await ResetEventSequenceAsync();

            _logger.LogInformation("Successfully seeded {Count} events", events.Count);
        }

        private async Task ResetVenueSequenceAsync()
        {
            try
            {
                var maxId = await _context.Venues.MaxAsync(v => (int?)v.VenueId) ?? 0;
                await _context.Database.ExecuteSqlAsync(
                    $"SELECT setval('venues_venue_id_seq', {maxId}, true)");

                _logger.LogInformation("Reset venues sequence to {MaxId}", maxId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reset venues sequence");
            }
        }

        private async Task ResetEventSequenceAsync()
        {
            try
            {
                var maxId = await _context.Events.MaxAsync(e => (int?)e.EventId) ?? 0;
                await _context.Database.ExecuteSqlAsync(
                    $"SELECT setval('events_event_id_seq', {maxId}, true)");

                _logger.LogInformation("Reset events sequence to {MaxId}", maxId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reset events sequence");
            }
        }
    }

    // CSV mapping classes
    public class VenueCsv
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int Capacity { get; set; }
    }

    public class EventCsv
    {
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public string Title { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; }
    }

    // CSV class maps
    public sealed class VenueCsvMap : ClassMap<VenueCsv>
    {
        public VenueCsvMap()
        {
            Map(m => m.VenueId).Name("venue_id");
            Map(m => m.Name).Name("name");
            Map(m => m.City).Name("city");
            Map(m => m.Capacity).Name("capacity");
        }
    }

    public sealed class EventCsvMap : ClassMap<EventCsv>
    {
        public EventCsvMap()
        {
            Map(m => m.EventId).Name("event_id");
            Map(m => m.VenueId).Name("venue_id");
            Map(m => m.Title).Name("title");
            Map(m => m.EventType).Name("event_type");
            Map(m => m.EventDate).Name("event_date")
                            .TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.BasePrice).Name("base_price");
            Map(m => m.Status).Name("status");
        }
    }
}
