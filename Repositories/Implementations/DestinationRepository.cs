using Microsoft.EntityFrameworkCore;
using TravelBookingApi.Data;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;

namespace TravelBookingApi.Repositories.Implementations
{
    public class DestinationRepository : IDestinationRepository
    {
        private readonly AppDbContext _context;

        public DestinationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Destination>> GetAllAsync()
        {
            return await _context.Destinations.ToListAsync();
        }

        public async Task<Destination> GetByIdAsync(int id)
        {
            return await _context.Destinations.FindAsync(id);
        }

        public async Task<Destination> AddAsync(Destination destination)
        {
            _context.Destinations.Add(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

        public async Task<Destination> UpdateAsync(Destination destination)
        {
            _context.Destinations.Update(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null) return false;

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
