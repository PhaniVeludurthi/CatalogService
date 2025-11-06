namespace CatalogService.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int Capacity { get; set; }
        // Navigation property
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
