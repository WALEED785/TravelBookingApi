using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<HotelDTO>> GetRecommendedHotelsAsync(int userId);
        Task<IEnumerable<FlightDTO>> GetRecommendedFlightsAsync(int userId);
    }
}