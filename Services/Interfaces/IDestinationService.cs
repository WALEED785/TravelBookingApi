using TravelBookingApi.Models.DTOs;

namespace TravelBookingApi.Services.Interfaces
{
    public interface IDestinationService
    {
        Task<IEnumerable<DestinationDTO>> GetAllDestinationsAsync();
        Task<DestinationDTO> GetDestinationByIdAsync(int id);
        Task<DestinationDTO> AddDestinationAsync(DestinationCreateDTO dto);
        Task<DestinationDTO> UpdateDestinationAsync(int id, DestinationCreateDTO dto);
        Task<bool> DeleteDestinationAsync(int id);
    }
}