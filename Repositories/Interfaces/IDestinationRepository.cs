using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Repositories.Interfaces
{
    public interface IDestinationRepository
    {
        Task<IEnumerable<Destination>> GetAllAsync();
        Task<Destination> GetByIdAsync(int id);
        Task<Destination> AddAsync(Destination destination);
        Task<Destination> UpdateAsync(Destination destination);
        Task<bool> DeleteAsync(int id);
    }
}