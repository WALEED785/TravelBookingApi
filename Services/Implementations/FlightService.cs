using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Implementations;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _repo;
        private readonly IDestinationRepository _destinationRepo;

        public FlightService(IFlightRepository repo, IDestinationRepository destinationRepo)
        {
            _repo = repo;
            _destinationRepo = destinationRepo;
        }

        /* ---------- Helpers ---------- */

        private static Flight FromCreateDto(FlightCreateDTO d) => new()
        {
            Airline = d.Airline,
            DepartureDestinationId = d.DepartureDestinationId,
            ArrivalDestinationId = d.ArrivalDestinationId,
            DepartureTime = d.DepartureTime,
            ArrivalTime = d.ArrivalTime,
            Price = d.Price
        };

        private static void Apply(FlightDTO d, Flight f)
        {
            f.Airline = d.Airline;
            f.DepartureDestinationId = d.DepartureDestinationId;
            f.ArrivalDestinationId = d.ArrivalDestinationId;
            f.DepartureTime = d.DepartureTime;
            f.ArrivalTime = d.ArrivalTime;
            f.Price = d.Price;
        }


        private async Task<FlightDTO> ToDtoWithDestinations(Flight f)
        {
            var departureDest = await _destinationRepo.GetByIdAsync(f.DepartureDestinationId);
            var arrivalDest = await _destinationRepo.GetByIdAsync(f.ArrivalDestinationId);

            return new FlightDTO
            {
                FlightId = f.FlightId,
                Airline = f.Airline,
                DepartureDestinationId = f.DepartureDestinationId,
                ArrivalDestinationId = f.ArrivalDestinationId,
                DepartureDestination = departureDest?.Name,
                ArrivalDestination = arrivalDest?.Name,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Price = f.Price
            };
        }
        private static FlightDTO MapToDTO(Flight flight)
        {
            return new FlightDTO
            {
                FlightId = flight.FlightId,
                Airline = flight.Airline,
                DepartureDestinationId = flight.DepartureDestinationId,
                DepartureTime = flight.DepartureTime,
                Price = flight.Price,
            };
        }


        /* ---------- CRUD ---------- */

        public async Task<IEnumerable<FlightDTO>> GetAllFlightsAsync()
        {
            var flights = await _repo.GetAllAsync();
            var flightDtos = new List<FlightDTO>();

            foreach (var flight in flights)
            {
                flightDtos.Add(await ToDtoWithDestinations(flight));
            }

            return flightDtos;
        }

        public async Task<FlightDTO?> GetFlightByIdAsync(int id)
        {
            var flight = await _repo.GetByIdAsync(id);
            if (flight is null) return null;

            return await ToDtoWithDestinations(flight);
        }

        public async Task<FlightDTO> AddFlightAsync(FlightCreateDTO dto)
        {
            var entity = FromCreateDto(dto);
            var saved = await _repo.AddAsync(entity);
            return await ToDtoWithDestinations(saved);
        }

        public async Task<FlightDTO?> UpdateFlightAsync(int id, FlightDTO dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return null;

            Apply(dto, entity);
            var updated = await _repo.UpdateAsync(entity);
            return await ToDtoWithDestinations(updated);
        }

        public Task<bool> DeleteFlightAsync(int id) => _repo.DeleteAsync(id);

        public async Task<IEnumerable<FlightDTO>> SearchFlightsAsync(int depId, int arrId, DateTime? date)
        {
            var flights = await _repo.SearchFlightsAsync(depId, arrId, date);
            var flightDtos = new List<FlightDTO>();

            foreach (var flight in flights)
            {
                flightDtos.Add(await ToDtoWithDestinations(flight));
            }

            return flightDtos;
        }
        public async Task<IEnumerable<FlightDTO>> GetFlightsByDestinationAsync(int destinationId)
        {
            var flights = await _repo.GetByDestinationAsync(destinationId);
            return flights.Select(h => MapToDTO(h));
        }

    }
}