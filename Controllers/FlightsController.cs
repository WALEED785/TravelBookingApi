using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController(IFlightService flightService) : ControllerBase
    {
        private readonly IFlightService _flightService = flightService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            return Ok(flights);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var flight = await _flightService.GetFlightByIdAsync(id);
            return Ok(flight);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FlightDTO flightDto)
        {
            var flight = await _flightService.AddFlightAsync(flightDto);
            return CreatedAtAction(nameof(GetById), new { id = flight.FlightId }, flight);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, FlightDTO flightDto)
        {
            var updatedFlight = await _flightService.UpdateFlightAsync(id, flightDto);
            return Ok(updatedFlight);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _flightService.DeleteFlightAsync(id);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights(int departureId, int arrivalId, DateTime? date)
        {
            var flights = await _flightService.SearchFlightsAsync(departureId, arrivalId, date);
            return Ok(flights);
        }
    }
}