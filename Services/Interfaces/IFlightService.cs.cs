// Services/Interfaces/IFlightService.cs
using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightDTO>> GetAllFlightsAsync();
        Task<FlightDTO> GetFlightByIdAsync(int id);
        Task<FlightDTO> AddFlightAsync(FlightDTO flightDto);
        Task<FlightDTO> UpdateFlightAsync(int id, FlightDTO flightDto);
        Task<bool> DeleteFlightAsync(int id);
        Task<IEnumerable<FlightDTO>> SearchFlightsAsync(int departureId, int arrivalId, DateTime? date);
    }
}