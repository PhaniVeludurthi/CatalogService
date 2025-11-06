using CatalogService.Dtos;
using CatalogService.Models;
using CatalogService.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VenuesController(IVenueRepository venueRepository, ILogger<VenuesController> logger) : ControllerBase
    {
        private readonly IVenueRepository _venueRepository = venueRepository;
        private readonly ILogger<VenuesController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetAll()
        {
            var venues = await _venueRepository.GetAllAsync();
            var venueDtos = venues.Select(MapToDto);
            return Ok(venueDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VenueDto>> GetById(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id);
            if (venue == null)
                return NotFound(new { message = $"Venue with ID {id} not found" });

            return Ok(MapToDto(venue));
        }

        [HttpGet("city/{city}")]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetByCity(string city)
        {
            var venues = await _venueRepository.GetByCityAsync(city);
            var venueDtos = venues.Select(MapToDto);
            return Ok(venueDtos);
        }

        [HttpPost]
        public async Task<ActionResult<VenueDto>> Create([FromBody] CreateVenueDto createDto)
        {
            var venue = new Venue
            {
                Name = createDto.Name,
                City = createDto.City,
                Capacity = createDto.Capacity
            };

            var created = await _venueRepository.CreateAsync(venue);
            var createdVenue = await _venueRepository.GetByIdAsync(created.VenueId);

            return CreatedAtAction(nameof(GetById), new { id = created.VenueId }, MapToDto(createdVenue!));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VenueDto>> Update(int id, [FromBody] CreateVenueDto updateDto)
        {
            var existing = await _venueRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Venue with ID {id} not found" });

            existing.Name = updateDto.Name;
            existing.City = updateDto.City;
            existing.Capacity = updateDto.Capacity;

            await _venueRepository.UpdateAsync(existing);
            var updated = await _venueRepository.GetByIdAsync(id);

            return Ok(MapToDto(updated!));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _venueRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Venue with ID {id} not found" });

            return NoContent();
        }

        private static VenueDto MapToDto(Venue venue)
        {
            return new VenueDto
            {
                VenueId = venue.VenueId,
                Name = venue.Name,
                City = venue.City,
                Capacity = venue.Capacity,
                EventCount = venue.Events?.Count ?? 0
            };
        }
    }
}
