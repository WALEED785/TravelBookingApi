// Repositories/Interfaces/IFlightRepository.cs
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Repositories.Interfaces
{
    public interface IFlightRepository
    {
        Task<IEnumerable<Flight>> GetAllAsync();
        Task<Flight?> GetByIdAsync(int id);
        Task<Flight> AddAsync(Flight flight);
        Task<Flight> UpdateAsync(Flight flight);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Flight>> SearchFlightsAsync(int departureId,
                                                     int arrivalId,
                                                     DateTime? date);
        Task<IEnumerable<Flight>> GetByDestinationAsync(int destinationId);
    }
}
