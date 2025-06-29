using AutoMapper;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public HotelService(IHotelRepository hotelRepository, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HotelDTO>> GetAllHotelsAsync()
        {
            var hotels = await _hotelRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<HotelDTO>>(hotels);
        }

        public async Task<HotelDTO> GetHotelByIdAsync(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            return _mapper.Map<HotelDTO>(hotel);
        }

        public async Task<HotelDTO> AddHotelAsync(HotelDTO hotelDto)
        {
            var hotel = _mapper.Map<Hotel>(hotelDto);
            var addedHotel = await _hotelRepository.AddAsync(hotel);
            return _mapper.Map<HotelDTO>(addedHotel);
        }

        public async Task<HotelDTO> UpdateHotelAsync(int id, HotelDTO hotelDto)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null) return null;

            _mapper.Map(hotelDto, hotel);
            var updatedHotel = await _hotelRepository.UpdateAsync(hotel);
            return _mapper.Map<HotelDTO>(updatedHotel);
        }

        public async Task<bool> DeleteHotelAsync(int id)
        {
            return await _hotelRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<HotelDTO>> GetHotelsByDestinationAsync(int destinationId)
        {
            var hotels = await _hotelRepository.GetByDestinationAsync(destinationId);
            return _mapper.Map<IEnumerable<HotelDTO>>(hotels);
        }
    }
}