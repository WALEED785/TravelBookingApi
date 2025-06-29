using AutoMapper;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public RecommendationService(
            IHotelRepository hotelRepository,
            IFlightRepository flightRepository,
            IBookingRepository bookingRepository,
            IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _flightRepository = flightRepository;
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HotelDTO>> GetRecommendedHotelsAsync(int userId)
        {
            // Simple recommendation logic (in practice, use ML/NLP)
            var userBookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var topDestinations = userBookings
                .Where(b => b.HotelId != null)
                .GroupBy(b => b.Hotel.DestinationId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key);

            var recommendedHotels = new List<Hotel>();
            foreach (var destId in topDestinations)
            {
                var hotels = await _hotelRepository.GetByDestinationAsync(destId);
                recommendedHotels.AddRange(hotels.OrderByDescending(h => h.Rating).Take(2));
            }

            return _mapper.Map<IEnumerable<HotelDTO>>(recommendedHotels);
        }

        public async Task<IEnumerable<FlightDTO>> GetRecommendedFlightsAsync(int userId)
        {
            // Simple recommendation logic (in practice, use ML)
            var userBookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var lastFlight = userBookings
                .Where(b => b.FlightId != null)
                .OrderByDescending(b => b.BookingDate)
                .FirstOrDefault();

            if (lastFlight == null) return new List<FlightDTO>();

            var recommendedFlights = await _flightRepository.SearchFlightsAsync(
                lastFlight.Flight.ArrivalDestinationId,
                lastFlight.Flight.DepartureDestinationId,
                DateTime.UtcNow.AddDays(7));

            return _mapper.Map<IEnumerable<FlightDTO>>(recommendedFlights);
        }
    }
}