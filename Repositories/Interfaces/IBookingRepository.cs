using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);

        Task<Booking> AddBookingAsync(Booking booking);
        Task<bool> CancelBookingAsync(int id);
    }
}
