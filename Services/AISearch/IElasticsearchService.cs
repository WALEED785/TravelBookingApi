using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Services.AISearch;

public interface IElasticsearchService
{
    Task<bool> CreateIndicesAsync();
    Task IndexDestinationAsync(ElasticDestination destination);
    Task IndexFlightAsync(ElasticFlight flight);
    Task IndexHotelAsync(ElasticHotel hotel);
    Task DeleteDestinationAsync(string id);
    Task DeleteFlightAsync(string id);
    Task DeleteHotelAsync(string id);

    // Search methods
    Task<SearchResultDto<ElasticDestination>> SearchDestinationsAsync(SearchRequestDto request);
    Task<SearchResultDto<ElasticFlight>> SearchFlightsAsync(SearchRequestDto request);
    Task<SearchResultDto<ElasticHotel>> SearchHotelsAsync(SearchRequestDto request);
    Task<IEnumerable<AutocompleteResultDto>> AutocompleteAsync(string query);

    // Bulk operations
    Task BulkIndexDestinationsAsync(IEnumerable<ElasticDestination> destinations);
    Task BulkIndexFlightsAsync(IEnumerable<ElasticFlight> flights);
    Task BulkIndexHotelsAsync(IEnumerable<ElasticHotel> hotels);
}