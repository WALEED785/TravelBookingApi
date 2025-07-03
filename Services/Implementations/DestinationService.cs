using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class DestinationService(IDestinationRepository destinationRepository) : IDestinationService
    {
        private readonly IDestinationRepository _destinationRepository = destinationRepository;

        public async Task<IEnumerable<DestinationDTO>> GetAllDestinationsAsync()
        {
            var destinations = await _destinationRepository.GetAllAsync();
            return destinations.Select(d => MapToDTO(d));
        }

        public async Task<DestinationDTO> GetDestinationByIdAsync(int id)
        {
            var destination = await _destinationRepository.GetByIdAsync(id);
            return destination == null ? null : MapToDTO(destination);
        }

        public async Task<DestinationDTO> AddDestinationAsync(DestinationCreateDTO dto)
        {
            var destination = MapToEntity(dto);
            var created = await _destinationRepository.AddAsync(destination);
            return MapToDTO(created);
        }

        public async Task<DestinationDTO> UpdateDestinationAsync(int id, DestinationCreateDTO dto)
        {
            var destination = await _destinationRepository.GetByIdAsync(id);
            if (destination == null) return null;

            destination.Name = dto.Name;
            destination.Country = dto.Country;
            destination.Description = dto.Description;

            var updated = await _destinationRepository.UpdateAsync(destination);
            return MapToDTO(updated);
        }

        public async Task<bool> DeleteDestinationAsync(int id)
        {
            return await _destinationRepository.DeleteAsync(id);
        }

        private static DestinationDTO MapToDTO(Destination d) => new DestinationDTO
        {
            DestinationId = d.DestinationId,
            Name = d.Name,
            Country = d.Country,
            Description = d.Description
        };

        private static Destination MapToEntity(DestinationCreateDTO dto) => new Destination
        {
            Name = dto.Name,
            Country = dto.Country,
            Description = dto.Description
        };
    }
}
