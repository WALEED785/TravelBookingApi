// Repositories/Implementations/FlightRepository.cs
using Microsoft.EntityFrameworkCore;
using TravelBookingApi.Data;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;

namespace TravelBookingApi.Repositories.Implementations
{
    public class FlightRepository(AppDbContext ctx) : IFlightRepository
    {
        private readonly AppDbContext _ctx = ctx;

        public async Task<IEnumerable<Flight>> GetAllAsync() =>
            await _ctx.Flights
                      .Include(f => f.DepartureDestination)
                      .Include(f => f.ArrivalDestination)
                      .ToListAsync();

        public async Task<Flight?> GetByIdAsync(int id) =>
            await _ctx.Flights
                      .Include(f => f.DepartureDestination)
                      .Include(f => f.ArrivalDestination)
                      .FirstOrDefaultAsync(f => f.FlightId == id);

        public async Task<Flight> AddAsync(Flight flight)
        {
            _ctx.Flights.Add(flight);
            await _ctx.SaveChangesAsync();
            return flight;
        }

        public async Task<Flight> UpdateAsync(Flight flight)
        {
            _ctx.Flights.Update(flight);
            await _ctx.SaveChangesAsync();
            return flight;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var flight = await GetByIdAsync(id);
            if (flight is null) return false;

            _ctx.Flights.Remove(flight);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(int departureId,
                                                                  int arrivalId,
                                                                  DateTime? date)
        {
            var q = _ctx.Flights
                        .Include(f => f.DepartureDestination)
                        .Include(f => f.ArrivalDestination)
                        .Where(f => f.DepartureDestinationId == departureId &&
                                    f.ArrivalDestinationId == arrivalId);

            if (date.HasValue)
                q = q.Where(f => f.DepartureTime.Date == date.Value.Date);

            return await q.ToListAsync();
        }
        public async Task<IEnumerable<Flight>> GetByDestinationAsync(int destinationId)
        {
            return await _ctx.Flights
                .Where(h => h.DepartureDestinationId == destinationId)
                .Include(h => h.DepartureDestination)
                .ToListAsync();
        }
    }
}
