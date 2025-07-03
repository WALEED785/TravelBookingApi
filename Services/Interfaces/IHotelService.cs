using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IHotelService
    {
        Task<IEnumerable<HotelDTO>> GetAllHotelsAsync();
        Task<HotelDTO> GetHotelByIdAsync(int id);
        Task<HotelDTO> AddHotelAsync(HotelCreateDTO hotelCreateDto);
        Task<HotelDTO> UpdateHotelAsync(int id, HotelCreateDTO hotelCreateDto);
        Task<bool> DeleteHotelAsync(int id);
        Task<IEnumerable<HotelDTO>> GetHotelsByDestinationAsync(int destinationId);
    }
}
