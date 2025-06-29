using TravelBookingApi.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();
        Task<BookingDTO> GetBookingByIdAsync(int id);
        Task<BookingDTO> CreateBookingAsync(BookingDTO bookingDto);
        Task<bool> CancelBookingAsync(int id);
        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId);
    }
}