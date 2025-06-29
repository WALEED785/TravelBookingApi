// Services/Implementations/FlightService.cs
using AutoMapper;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IMapper _mapper;

        public FlightService(IFlightRepository flightRepository, IMapper mapper)
        {
            _flightRepository = flightRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FlightDTO>> GetAllFlightsAsync()
        {
            var flights = await _flightRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<FlightDTO>>(flights);
        }

        public async Task<FlightDTO> GetFlightByIdAsync(int id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            return _mapper.Map<FlightDTO>(flight);
        }

        public async Task<FlightDTO> AddFlightAsync(FlightDTO flightDto)
        {
            var flight = _mapper.Map<Flight>(flightDto);
            var addedFlight = await _flightRepository.AddAsync(flight);
            return _mapper.Map<FlightDTO>(addedFlight);
        }

        public async Task<FlightDTO> UpdateFlightAsync(int id, FlightDTO flightDto)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return null;

            _mapper.Map(flightDto, flight);
            var updatedFlight = await _flightRepository.UpdateAsync(flight);
            return _mapper.Map<FlightDTO>(updatedFlight);
        }

        public async Task<bool> DeleteFlightAsync(int id)
        {
            return await _flightRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<FlightDTO>> SearchFlightsAsync(int departureId, int arrivalId, DateTime? date)
        {
            var flights = await _flightRepository.SearchFlightsAsync(departureId, arrivalId, date);
            return _mapper.Map<IEnumerable<FlightDTO>>(flights);
        }
    }
}