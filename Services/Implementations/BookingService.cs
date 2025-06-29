using AutoMapper;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TravelBookingApi.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public BookingService(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookingDTO>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync();
            return _mapper.Map<IEnumerable<BookingDTO>>(bookings);
        }

        public async Task<BookingDTO> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);
            return _mapper.Map<BookingDTO>(booking);
        }

        public async Task<BookingDTO> CreateBookingAsync(BookingDTO bookingDto)
        {
            var booking = _mapper.Map<Booking>(bookingDto);
            var createdBooking = await _bookingRepository.AddBookingAsync(booking);
            return _mapper.Map<BookingDTO>(createdBooking);
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            return await _bookingRepository.CancelBookingAsync(id);
        }

        public async Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            return _mapper.Map<IEnumerable<BookingDTO>>(bookings);
        }
    }
}