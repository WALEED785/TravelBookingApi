using Microsoft.EntityFrameworkCore;
using TravelBookingApi.Data;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;

namespace TravelBookingApi.Repositories.Implementations
{
    public class FlightRepository(AppDbContext context) : IFlightRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<Flight>> GetAllAsync()
        {
            return await _context.Flights
                .Include(f => f.DepartureDestination)
                .Include(f => f.ArrivalDestination)
                .ToListAsync();
        }

        public async Task<Flight> GetByIdAsync(int id)
        {
            return await _context.Flights
                .Include(f => f.DepartureDestination)
                .Include(f => f.ArrivalDestination)
                .FirstOrDefaultAsync(f => f.FlightId == id);
        }

        public async Task<Flight> AddAsync(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task<Flight> UpdateAsync(Flight flight)
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var flight = await GetByIdAsync(id);
            if (flight == null) return false;

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(int departureId, int arrivalId, DateTime? date)
        {
            var query = _context.Flights
                .Include(f => f.DepartureDestination)
                .Include(f => f.ArrivalDestination)
                .Where(f => f.DepartureDestinationId == departureId && f.ArrivalDestinationId == arrivalId);

            if (date.HasValue)
            {
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);
            }

            return await query.ToListAsync();
        }
    }
}