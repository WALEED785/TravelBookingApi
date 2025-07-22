using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Services.AISearch;

namespace TravelBookingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElasticsearchController : ControllerBase
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<ElasticsearchController> _logger;

        public ElasticsearchController(IElasticsearchService elasticsearchService, ILogger<ElasticsearchController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        /// <summary>
        /// Initialize Elasticsearch indices
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeIndices()
        {
            try
            {
                var result = await _elasticsearchService.CreateIndicesAsync();
                if (result)
                {
                    return Ok(new { message = "Elasticsearch indices created successfully" });
                }
                return BadRequest(new { message = "Failed to create Elasticsearch indices" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Elasticsearch indices");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Search destinations
        /// </summary>
        [HttpGet("search/destinations")]
        public async Task<IActionResult> SearchDestinations([FromQuery] SearchRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var results = await _elasticsearchService.SearchDestinationsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching destinations");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Search flights
        /// </summary>
        [HttpGet("search/flights")]
        public async Task<IActionResult> SearchFlights([FromQuery] SearchRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var results = await _elasticsearchService.SearchFlightsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Search hotels
        /// </summary>
        [HttpGet("search/hotels")]
        public async Task<IActionResult> SearchHotels([FromQuery] SearchRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var results = await _elasticsearchService.SearchHotelsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Autocomplete search
        /// </summary>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query) || query.Length < 2)
                {
                    return BadRequest(new { message = "Query must be at least 2 characters long" });
                }

                var results = await _elasticsearchService.AutocompleteAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in autocomplete");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Index a destination
        /// </summary>
        [HttpPost("index/destination")]
        public async Task<IActionResult> IndexDestination([FromBody] ElasticDestination destination)
        {
            try
            {
                if (destination == null)
                {
                    return BadRequest(new { message = "Destination data is required" });
                }

                await _elasticsearchService.IndexDestinationAsync(destination);
                return Ok(new { message = "Destination indexed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing destination");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Index a flight
        /// </summary>
        [HttpPost("index/flight")]
        public async Task<IActionResult> IndexFlight([FromBody] ElasticFlight flight)
        {
            try
            {
                if (flight == null)
                {
                    return BadRequest(new { message = "Flight data is required" });
                }

                await _elasticsearchService.IndexFlightAsync(flight);
                return Ok(new { message = "Flight indexed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing flight");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Index a hotel
        /// </summary>
        [HttpPost("index/hotel")]
        public async Task<IActionResult> IndexHotel([FromBody] ElasticHotel hotel)
        {
            try
            {
                if (hotel == null)
                {
                    return BadRequest(new { message = "Hotel data is required" });
                }

                await _elasticsearchService.IndexHotelAsync(hotel);
                return Ok(new { message = "Hotel indexed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing hotel");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk index destinations
        /// </summary>
        [HttpPost("bulk/destinations")]
        public async Task<IActionResult> BulkIndexDestinations([FromBody] IEnumerable<ElasticDestination> destinations)
        {
            try
            {
                if (destinations == null || !destinations.Any())
                {
                    return BadRequest(new { message = "Destinations data is required" });
                }

                await _elasticsearchService.BulkIndexDestinationsAsync(destinations);
                return Ok(new { message = $"Bulk indexed {destinations.Count()} destinations successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing destinations");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk index flights
        /// </summary>
        [HttpPost("bulk/flights")]
        public async Task<IActionResult> BulkIndexFlights([FromBody] IEnumerable<ElasticFlight> flights)
        {
            try
            {
                if (flights == null || !flights.Any())
                {
                    return BadRequest(new { message = "Flights data is required" });
                }

                await _elasticsearchService.BulkIndexFlightsAsync(flights);
                return Ok(new { message = $"Bulk indexed {flights.Count()} flights successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing flights");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk index hotels
        /// </summary>
        [HttpPost("bulk/hotels")]
        public async Task<IActionResult> BulkIndexHotels([FromBody] IEnumerable<ElasticHotel> hotels)
        {
            try
            {
                if (hotels == null || !hotels.Any())
                {
                    return BadRequest(new { message = "Hotels data is required" });
                }

                await _elasticsearchService.BulkIndexHotelsAsync(hotels);
                return Ok(new { message = $"Bulk indexed {hotels.Count()} hotels successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing hotels");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a destination from index
        /// </summary>
        [HttpDelete("destination/{id}")]
        public async Task<IActionResult> DeleteDestination(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Destination ID is required" });
                }

                await _elasticsearchService.DeleteDestinationAsync(id);
                return Ok(new { message = "Destination deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting destination");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a flight from index
        /// </summary>
        [HttpDelete("flight/{id}")]
        public async Task<IActionResult> DeleteFlight(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Flight ID is required" });
                }

                await _elasticsearchService.DeleteFlightAsync(id);
                return Ok(new { message = "Flight deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flight");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a hotel from index
        /// </summary>
        [HttpDelete("hotel/{id}")]
        public async Task<IActionResult> DeleteHotel(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "Hotel ID is required" });
                }

                await _elasticsearchService.DeleteHotelAsync(id);
                return Ok(new { message = "Hotel deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}