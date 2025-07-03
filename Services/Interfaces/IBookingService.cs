using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();
        Task<BookingDTO?> GetBookingByIdAsync(int id);
        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId);

        Task<BookingDTO> CreateBookingAsync(BookingCreateDTO dto);
        Task<bool> CancelBookingAsync(int id);
    }
}
