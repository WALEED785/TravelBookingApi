using Microsoft.EntityFrameworkCore;
using TravelBookingApi.Data;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;

namespace TravelBookingApi.Repositories.Implementations
{
    public sealed class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _ctx;

        public BookingRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync() =>
            await _ctx.Bookings
                      .Include(b => b.User)
                      .Include(b => b.Hotel)
                      .Include(b => b.Flight)
                      .ToListAsync();

        public async Task<Booking?> GetBookingByIdAsync(int id) =>
            await _ctx.Bookings
                      .Include(b => b.User)
                      .Include(b => b.Hotel)
                      .Include(b => b.Flight)
                      .FirstOrDefaultAsync(b => b.BookingId == id);

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId) =>
            await _ctx.Bookings
                      .Where(b => b.UserId == userId)
                      .Include(b => b.Hotel)
                      .Include(b => b.Flight)
                      .ToListAsync();

        public async Task<Booking> AddBookingAsync(Booking booking)
        {
            _ctx.Bookings.Add(booking);
            await _ctx.SaveChangesAsync();
            return booking;                         // DB now has BookingId
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            var entity = await _ctx.Bookings.FindAsync(id);
            if (entity is null) return false;

            entity.Status = "Cancelled";
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
