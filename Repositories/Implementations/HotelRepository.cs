using Microsoft.EntityFrameworkCore;
using TravelBookingApi.Data;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;

namespace TravelBookingApi.Repositories.Implementations
{
    public class HotelRepository(AppDbContext context) : IHotelRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<Hotel>> GetAllAsync()
        {
            return await _context.Hotels.Include(h => h.Destination).ToListAsync();
        }

        public async Task<Hotel> GetByIdAsync(int id)
        {
            return await _context.Hotels
                .Include(h => h.Destination)
                .FirstOrDefaultAsync(h => h.HotelId == id);
        }

        public async Task<Hotel> AddAsync(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        public async Task<Hotel> UpdateAsync(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hotel = await GetByIdAsync(id);
            if (hotel == null) return false;

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Hotel>> GetByDestinationAsync(int destinationId)
        {
            return await _context.Hotels
                .Where(h => h.DestinationId == destinationId)
                .Include(h => h.Destination)
                .ToListAsync();
        }
    }
}