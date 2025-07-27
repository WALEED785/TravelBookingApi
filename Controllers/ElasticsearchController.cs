using Microsoft.AspNetCore.Mvc;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Services.AISearch;

namespace TravelBookingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IElasticsearchService elasticsearchService, ILogger<SearchController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        /// <summary>
        /// Search destinations - checks Elasticsearch first, then database if not found
        /// </summary>
        [HttpGet("destinations")]
        public async Task<ActionResult<SearchResultDto<ElasticDestination>>> SearchDestinations(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var searchRequest = new SearchRequestDto
                {
                    Query = query.Trim(),
                    Page = page,
                    PageSize = pageSize
                };

                var results = await _elasticsearchService.SearchDestinationsAsync(searchRequest);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching destinations for query: {Query}", query);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Search flights - checks Elasticsearch first, then database if not found
        /// </summary>
        [HttpGet("flights")]
        public async Task<ActionResult<SearchResultDto<ElasticFlight>>> SearchFlights(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var searchRequest = new SearchRequestDto
                {
                    Query = query.Trim(),
                    Page = page,
                    PageSize = pageSize
                };

                var results = await _elasticsearchService.SearchFlightsAsync(searchRequest);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights for query: {Query}", query);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Search hotels - checks Elasticsearch first, then database if not found
        /// </summary>
        [HttpGet("hotels")]
        public async Task<ActionResult<SearchResultDto<ElasticHotel>>> SearchHotels(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var searchRequest = new SearchRequestDto
                {
                    Query = query.Trim(),
                    Page = page,
                    PageSize = pageSize
                };

                var results = await _elasticsearchService.SearchHotelsAsync(searchRequest);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels for query: {Query}", query);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Autocomplete search across destinations and hotels
        /// </summary>
        [HttpGet("autocomplete")]
        public async Task<ActionResult<IEnumerable<AutocompleteResultDto>>> Autocomplete([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                {
                    return BadRequest(new { message = "Query must be at least 2 characters long" });
                }

                var results = await _elasticsearchService.AutocompleteAsync(query.Trim());
                return Ok(new { query = query.Trim(), suggestions = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in autocomplete for query: {Query}", query);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
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
        /// Health check for Elasticsearch
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    message = "Search service is operational"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search service health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }
    }
}