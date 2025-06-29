using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IHotelService
    {
        Task<IEnumerable<HotelDTO>> GetAllHotelsAsync();
        Task<HotelDTO> GetHotelByIdAsync(int id);
        Task<HotelDTO> AddHotelAsync(HotelDTO hotelDto);
        Task<HotelDTO> UpdateHotelAsync(int id, HotelDTO hotelDto);
        Task<bool> DeleteHotelAsync(int id);
        Task<IEnumerable<HotelDTO>> GetHotelsByDestinationAsync(int destinationId);
    }
}