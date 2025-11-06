using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Context
{
    public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
    {
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Venue Configuration
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.ToTable("venues");

                entity.HasKey(e => e.VenueId);

                entity.Property(e => e.VenueId)
                    .HasColumnName("venue_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Capacity)
                    .HasColumnName("capacity")
                    .IsRequired();

                // Indexes
                entity.HasIndex(e => e.City)
                    .HasDatabaseName("idx_venues_city");

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("idx_venues_name");
            });

            // Event Configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("events");

                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EventId)
                    .HasColumnName("event_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.VenueId)
                    .HasColumnName("venue_id")
                    .IsRequired();

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(e => e.EventType)
                    .HasColumnName("event_type")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.EventDate)
                    .HasColumnName("event_date")
                 .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(e => e.BasePrice)
                    .HasColumnName("base_price")
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(50)
                    .IsRequired();

                // Foreign Key Relationship
                entity.HasOne(e => e.Venue)
                    .WithMany(v => v.Events)
                    .HasForeignKey(e => e.VenueId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_events_venue");

                // Indexes
                entity.HasIndex(e => e.VenueId)
                    .HasDatabaseName("idx_events_venue_id");

                entity.HasIndex(e => e.EventDate)
                    .HasDatabaseName("idx_events_event_date");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_events_status");

                entity.HasIndex(e => e.EventType)
                    .HasDatabaseName("idx_events_event_type");
            });
        }
    }
}
