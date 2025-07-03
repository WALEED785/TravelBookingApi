using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationsController(IDestinationService destinationService) : ControllerBase
    {
        private readonly IDestinationService _destinationService = destinationService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var destinations = await _destinationService.GetAllDestinationsAsync();
            return Ok(destinations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null) return NotFound();
            return Ok(destination);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DestinationCreateDTO destinationCreateDto)
        {
            var createdDestination = await _destinationService.AddDestinationAsync(destinationCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = createdDestination.DestinationId }, createdDestination);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DestinationCreateDTO destinationCreateDto)
        {
            var updatedDestination = await _destinationService.UpdateDestinationAsync(id, destinationCreateDto);
            if (updatedDestination == null) return NotFound();
            return Ok(updatedDestination);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _destinationService.DeleteDestinationAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
