using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("hotels/{userId}")]
        public async Task<IActionResult> GetRecommendedHotels(int userId)
        {
            var hotels = await _recommendationService.GetRecommendedHotelsAsync(userId);
            return Ok(hotels);
        }

        [HttpGet("flights/{userId}")]
        public async Task<IActionResult> GetRecommendedFlights(int userId)
        {
            var flights = await _recommendationService.GetRecommendedFlightsAsync(userId);
            return Ok(flights);
        }
    }
}