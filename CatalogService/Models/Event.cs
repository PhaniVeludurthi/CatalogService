
namespace CatalogService.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public string Title { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; }
        public DateTime? CancelledAt { get; set; }
        // Navigation property
        public Venue Venue { get; set; } = null!;
    }
}
