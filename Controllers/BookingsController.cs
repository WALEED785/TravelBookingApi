using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Services.Interfaces;
using System.Threading.Tasks;

namespace TravelBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateDTO bookingDto)
        {
            var booking = await _bookingService.CreateBookingAsync(bookingDto);
            return CreatedAtAction(nameof(GetById),
                new { id = booking.BookingId },
                booking);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBookings(int userId)
        {
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }
    }
}