namespace CatalogService.Dtos
{
    public class VenueDto
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int Capacity { get; set; }
        public int EventCount { get; set; }
    }
    public class CreateVenueDto
    {
        public string Name { get; set; }
        public string City { get; set; }
        public int Capacity { get; set; }
    }
}
