using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class HotelService(IHotelRepository hotelRepository) : IHotelService
    {
        private readonly IHotelRepository _hotelRepository = hotelRepository;

        public async Task<IEnumerable<HotelDTO>> GetAllHotelsAsync()
        {
            var hotels = await _hotelRepository.GetAllAsync();
            return hotels.Select(h => MapToDTO(h));
        }

        public async Task<HotelDTO> GetHotelByIdAsync(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            return hotel == null ? null : MapToDTO(hotel);
        }

        public async Task<HotelDTO> AddHotelAsync(HotelCreateDTO hotelCreateDto)
        {
            var hotel = MapToEntity(hotelCreateDto);
            var addedHotel = await _hotelRepository.AddAsync(hotel);
            return MapToDTO(addedHotel);
        }

        public async Task<HotelDTO> UpdateHotelAsync(int id, HotelCreateDTO hotelCreateDto)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null) return null;

            // Update fields manually
            hotel.Name = hotelCreateDto.Name;
            hotel.DestinationId = hotelCreateDto.DestinationId;
            hotel.PricePerNight = hotelCreateDto.PricePerNight;
            hotel.Rating = hotelCreateDto.Rating;
            hotel.Amenities = hotelCreateDto.Amenities;

            var updatedHotel = await _hotelRepository.UpdateAsync(hotel);
            return MapToDTO(updatedHotel);
        }

        public async Task<bool> DeleteHotelAsync(int id)
        {
            return await _hotelRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<HotelDTO>> GetHotelsByDestinationAsync(int destinationId)
        {
            var hotels = await _hotelRepository.GetByDestinationAsync(destinationId);
            return hotels.Select(h => MapToDTO(h));
        }

        private static HotelDTO MapToDTO(Hotel hotel)
        {
            return new HotelDTO
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                DestinationId = hotel.DestinationId,
                PricePerNight = hotel.PricePerNight,
                Rating = hotel.Rating,
                Amenities = hotel.Amenities
            };
        }

        private static Hotel MapToEntity(HotelCreateDTO hotelCreateDto)
        {
            return new Hotel
            {
                Name = hotelCreateDto.Name,
                DestinationId = hotelCreateDto.DestinationId,
                PricePerNight = hotelCreateDto.PricePerNight,
                Rating = hotelCreateDto.Rating,
                Amenities = hotelCreateDto.Amenities
            };
        }
    }
}
