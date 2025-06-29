// Services/Interfaces/IUserService.cs
using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetAllAsync();
        Task<UserDTO> GetByIdAsync(int id);
        Task<string> RegisterAsync(UserRegisterDTO registerDto);
        Task<LoginResponseDTO> LoginAsync(UserLoginDTO loginDto);
        Task UpdateAsync(int id, UserUpdateDTO updateDto);
        Task DeleteAsync(int id);
    }
}