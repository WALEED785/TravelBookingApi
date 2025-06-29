// Repositories/Interfaces/IUserRepository.cs
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByTokenAsync(string token);
        Task<bool> UserExistsAsync(string username);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}