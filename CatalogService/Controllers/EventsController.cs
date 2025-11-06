using CatalogService.Dtos;
using CatalogService.Models;
using CatalogService.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EventsController(IEventRepository eventRepository, ILogger<EventsController> logger) : ControllerBase
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly ILogger<EventsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
        {
            var events = await _eventRepository.GetAllAsync();
            var eventDtos = events.Select(MapToDto);
            return Ok(eventDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetById(int id)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            if (eventEntity == null)
                return NotFound(new { message = $"Event with ID {id} not found" });

            return Ok(MapToDto(eventEntity));
        }

        [HttpGet("venue/{venueId}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetByVenue(int venueId)
        {
            var events = await _eventRepository.GetByVenueIdAsync(venueId);
            var eventDtos = events.Select(MapToDto);
            return Ok(eventDtos);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetByStatus(string status)
        {
            var events = await _eventRepository.GetByStatusAsync(status);
            var eventDtos = events.Select(MapToDto);
            return Ok(eventDtos);
        }

        [HttpGet("city/{city}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetByCity(string city)
        {
            var events = await _eventRepository.GetByCityAsync(city);
            var eventDtos = events.Select(MapToDto);
            return Ok(eventDtos);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EventDto>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var events = await _eventRepository.SearchAsync(query);
            var eventDtos = events.Select(MapToDto);
            return Ok(eventDtos);
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventDto createDto)
        {
            var eventEntity = new Event
            {
                VenueId = createDto.VenueId,
                Title = createDto.Title,
                EventType = createDto.EventType,
                EventDate = createDto.EventDate,
                BasePrice = createDto.BasePrice,
                Status = createDto.Status
            };

            var created = await _eventRepository.CreateAsync(eventEntity);
            var createdEvent = await _eventRepository.GetByIdAsync(created.EventId);

            return CreatedAtAction(nameof(GetById), new { id = created.EventId }, MapToDto(createdEvent!));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EventDto>> Update(int id, [FromBody] UpdateEventDto updateDto)
        {
            var existing = await _eventRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Event with ID {id} not found" });

            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                existing.Title = updateDto.Title;
            if (!string.IsNullOrWhiteSpace(updateDto.EventType))
                existing.EventType = updateDto.EventType;
            if (updateDto.EventDate.HasValue)
                existing.EventDate = updateDto.EventDate.Value;
            if (updateDto.BasePrice.HasValue)
                existing.BasePrice = updateDto.BasePrice.Value;
            if (!string.IsNullOrWhiteSpace(updateDto.Status))
                existing.Status = updateDto.Status;

            await _eventRepository.UpdateAsync(existing);
            var updated = await _eventRepository.GetByIdAsync(id);

            return Ok(MapToDto(updated!));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _eventRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Event with ID {id} not found" });

            return NoContent();
        }

        private static EventDto MapToDto(Event eventEntity)
        {
            return new EventDto
            {
                EventId = eventEntity.EventId,
                VenueId = eventEntity.VenueId,
                Title = eventEntity.Title,
                EventType = eventEntity.EventType,
                EventDate = eventEntity.EventDate,
                BasePrice = eventEntity.BasePrice,
                Status = eventEntity.Status,
                VenueName = eventEntity.Venue?.Name,
                City = eventEntity.Venue?.City
            };
        }
    }
}
