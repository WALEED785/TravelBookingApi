using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public sealed class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;

        public BookingService(IBookingRepository repo)
        {
            _repo = repo;
        }

        /* ---------- READ ---------- */

        public async Task<IEnumerable<BookingDTO>> GetAllBookingsAsync()
        {
            var list = await _repo.GetAllBookingsAsync();
            return list.Select(ToDto);
        }

        public async Task<BookingDTO?> GetBookingByIdAsync(int id)
        {
            var entity = await _repo.GetBookingByIdAsync(id);
            return entity is null ? null : ToDto(entity);
        }

        public async Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId)
        {
            var list = await _repo.GetUserBookingsAsync(userId);
            return list.Select(ToDto);
        }

        /* ---------- CREATE ---------- */

        public async Task<BookingDTO> CreateBookingAsync(BookingCreateDTO dto)
        {
            var entity = FromCreateDto(dto);
            var saved = await _repo.AddBookingAsync(entity);
            return ToDto(saved);
        }

        /* ---------- UPDATE ---------- */

        public Task<bool> CancelBookingAsync(int id) =>
            _repo.CancelBookingAsync(id);

        /* ---------- PRIVATE MAPPERS ---------- */

        private static Booking FromCreateDto(BookingCreateDTO dto) => new()
        {
            UserId = dto.UserId,
            HotelId = dto.HotelId,
            FlightId = dto.FlightId,
            TotalPrice = dto.TotalPrice,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Confirmed" : dto.Status
            // BookingDate will default to UtcNow via entity initializer
        };

        private static BookingDTO ToDto(Booking b) => new()
        {
            BookingId = b.BookingId,
            UserId = b.UserId,
            HotelId = b.HotelId,
            FlightId = b.FlightId,
            HotelName = b.Hotel?.Name,
            FlightName = b.Flight?.Airline,
            TotalPrice = b.TotalPrice,
            Status = b.Status
        };
    }
}
