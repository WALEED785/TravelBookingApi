// Controllers/FlightsController.cs
using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Services.Implementations;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController(IFlightService svc) : ControllerBase
    {
        private readonly IFlightService _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _svc.GetAllFlightsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            Ok(await _svc.GetFlightByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FlightCreateDTO dto)
        {
            var created = await _svc.AddFlightAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                   new { id = created.FlightId },
                                   created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FlightDTO dto)
        {
            var updated = await _svc.UpdateFlightAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) =>
            (await _svc.DeleteFlightAsync(id)) ? NoContent() : NotFound();

        [HttpGet("search")]
        public async Task<IActionResult> Search(int departureId,
                                                int arrivalId,
                                                DateTime? date) =>
            Ok(await _svc.SearchFlightsAsync(departureId, arrivalId, date));

        [HttpGet("destination/{destinationId}")]
        public async Task<IActionResult> GetByDestination(int destinationId)
        {
            var flight = await _svc.GetFlightsByDestinationAsync(destinationId);
            return Ok(flight);
        }

    }
}
