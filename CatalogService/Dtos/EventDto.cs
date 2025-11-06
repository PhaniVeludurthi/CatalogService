
namespace CatalogService.Dtos
{
    public class EventDto
    {
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public string Title { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; }
        public string? VenueName { get; set; }
        public string? City { get; set; }
    }
    public class CreateEventDto
    {
        public int VenueId { get; set; }
        public string Title { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; }
    }
    public class UpdateEventDto
    {
        public string? Title { get; set; }
        public string EventType { get; set; }
        public DateTime? EventDate { get; set; }
        public decimal? BasePrice { get; set; }
        public string Status { get; set; }
    }
}
