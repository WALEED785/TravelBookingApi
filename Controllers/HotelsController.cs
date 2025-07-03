using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController(IHotelService hotelService) : ControllerBase
    {
        private readonly IHotelService _hotelService = hotelService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            return Ok(hotels);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null) return NotFound();
            return Ok(hotel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(HotelCreateDTO hotelCreateDto)
        {
            var hotel = await _hotelService.AddHotelAsync(hotelCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = hotel.HotelId }, hotel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, HotelCreateDTO hotelCreateDto)
        {
            var updatedHotel = await _hotelService.UpdateHotelAsync(id, hotelCreateDto);
            if (updatedHotel == null) return NotFound();
            return Ok(updatedHotel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _hotelService.DeleteHotelAsync(id);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("destination/{destinationId}")]
        public async Task<IActionResult> GetByDestination(int destinationId)
        {
            var hotels = await _hotelService.GetHotelsByDestinationAsync(destinationId);
            return Ok(hotels);
        }
    }
}
