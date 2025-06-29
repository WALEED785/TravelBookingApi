using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Repositories.Interfaces
{
    public interface IHotelRepository
    {
        Task<IEnumerable<Hotel>> GetAllAsync();
        Task<Hotel> GetByIdAsync(int id);
        Task<Hotel> AddAsync(Hotel hotel);
        Task<Hotel> UpdateAsync(Hotel hotel);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Hotel>> GetByDestinationAsync(int destinationId);
    }
}