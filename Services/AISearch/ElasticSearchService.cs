using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBookingApi.Data;
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Services.AISearch
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticsearchService> _logger;
        private readonly AppDbContext _dbContext;

        public ElasticsearchService(
            IElasticClient elasticClient,
            ILogger<ElasticsearchService> logger,
            AppDbContext dbContext)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _dbContext = dbContext;
        }

        #region Index Management

        public async Task<bool> CreateIndicesAsync()
        {
            try
            {
                // Check Elasticsearch connection
                var pingResponse = await _elasticClient.PingAsync();
                if (!pingResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch is not available: {Error}", pingResponse.DebugInformation);
                    return false;
                }

                // Create indices
                await CreateDestinationIndexAsync();
                await CreateFlightIndexAsync();
                await CreateHotelIndexAsync();

                _logger.LogInformation("All Elasticsearch indices created successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Elasticsearch indices");
                return false;
            }
        }

        private async Task CreateDestinationIndexAsync()
        {
            var indexExists = await _elasticClient.Indices.ExistsAsync("destinations");
            if (!indexExists.Exists)
            {
                await _elasticClient.Indices.CreateAsync("destinations", c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                        .Analysis(a => a
                            .Analyzers(an => an
                                .Custom("autocomplete", ca => ca
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "autocomplete_filter")
                                )
                            )
                            .TokenFilters(tf => tf
                                .EdgeNGram("autocomplete_filter", e => e
                                    .MinGram(2)
                                    .MaxGram(20)
                                )
                            )
                        )
                    )
                    .Map<ElasticDestination>(m => m.AutoMap())
                );
            }
        }

        private async Task CreateFlightIndexAsync()
        {
            var indexExists = await _elasticClient.Indices.ExistsAsync("flights");
            if (!indexExists.Exists)
            {
                await _elasticClient.Indices.CreateAsync("flights", c => c
                    .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0))
                    .Map<ElasticFlight>(m => m.AutoMap())
                );
            }
        }

        private async Task CreateHotelIndexAsync()
        {
            var indexExists = await _elasticClient.Indices.ExistsAsync("hotels");
            if (!indexExists.Exists)
            {
                await _elasticClient.Indices.CreateAsync("hotels", c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                        .Analysis(a => a
                            .Analyzers(an => an
                                .Custom("autocomplete", ca => ca
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "autocomplete_filter")
                                )
                            )
                            .TokenFilters(tf => tf
                                .EdgeNGram("autocomplete_filter", e => e
                                    .MinGram(2)
                                    .MaxGram(20)
                                )
                            )
                        )
                    )
                    .Map<ElasticHotel>(m => m.AutoMap())
                );
            }
        }

        #endregion

        #region Search Methods with DB Fallback

        public async Task<SearchResultDto<ElasticDestination>> SearchDestinationsAsync(SearchRequestDto request)
        {
            try
            {
                // First, try to search in Elasticsearch
                var elasticResults = await SearchDestinationsInElasticAsync(request);

                // If results found in Elastic, return them
                if (elasticResults.Total > 0)
                {
                    _logger.LogInformation("Found {Count} destinations in Elasticsearch for query: {Query}",
                        elasticResults.Total, request.Query);
                    return elasticResults;
                }

                // If no results in Elastic, query database and index the results
                _logger.LogInformation("No results in Elasticsearch, querying database for: {Query}", request.Query);

                var dbDestinations = await GetDestinationsFromDatabaseAsync(request.Query);

                if (dbDestinations.Any())
                {
                    // Convert to Elastic entities
                    var elasticDestinations = dbDestinations.Select(ConvertToElasticDestination).ToList();

                    // Bulk index them
                    await BulkIndexDestinationsAsync(elasticDestinations);

                    // Return paginated results
                    var pagedResults = elasticDestinations
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                    return new SearchResultDto<ElasticDestination>
                    {
                        Results = pagedResults,
                        Total = elasticDestinations.Count,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }

                // No results found anywhere
                return new SearchResultDto<ElasticDestination>
                {
                    Results = new List<ElasticDestination>(),
                    Total = 0,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching destinations");
                throw;
            }
        }

        public async Task<SearchResultDto<ElasticFlight>> SearchFlightsAsync(SearchRequestDto request)
        {
            try
            {
                // First, try to search in Elasticsearch
                var elasticResults = await SearchFlightsInElasticAsync(request);

                if (elasticResults.Total > 0)
                {
                    _logger.LogInformation("Found {Count} flights in Elasticsearch for query: {Query}",
                        elasticResults.Total, request.Query);
                    return elasticResults;
                }

                // Query database and index results
                _logger.LogInformation("No results in Elasticsearch, querying database for flights: {Query}", request.Query);

                var dbFlights = await GetFlightsFromDatabaseAsync(request.Query);

                if (dbFlights.Any())
                {
                    var elasticFlights = dbFlights.Select(ConvertToElasticFlight).ToList();
                    await BulkIndexFlightsAsync(elasticFlights);

                    var pagedResults = elasticFlights
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                    return new SearchResultDto<ElasticFlight>
                    {
                        Results = pagedResults,
                        Total = elasticFlights.Count,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }

                return new SearchResultDto<ElasticFlight>
                {
                    Results = new List<ElasticFlight>(),
                    Total = 0,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights");
                throw;
            }
        }

        public async Task<SearchResultDto<ElasticHotel>> SearchHotelsAsync(SearchRequestDto request)
        {
            try
            {
                // First, try to search in Elasticsearch
                var elasticResults = await SearchHotelsInElasticAsync(request);

                if (elasticResults.Total > 0)
                {
                    _logger.LogInformation("Found {Count} hotels in Elasticsearch for query: {Query}",
                        elasticResults.Total, request.Query);
                    return elasticResults;
                }

                // Query database and index results
                _logger.LogInformation("No results in Elasticsearch, querying database for hotels: {Query}", request.Query);

                var dbHotels = await GetHotelsFromDatabaseAsync(request.Query);

                if (dbHotels.Any())
                {
                    var elasticHotels = dbHotels.Select(ConvertToElasticHotel).ToList();
                    await BulkIndexHotelsAsync(elasticHotels);

                    var pagedResults = elasticHotels
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                    return new SearchResultDto<ElasticHotel>
                    {
                        Results = pagedResults,
                        Total = elasticHotels.Count,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }

                return new SearchResultDto<ElasticHotel>
                {
                    Results = new List<ElasticHotel>(),
                    Total = 0,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                throw;
            }
        }

        #endregion

        #region Private Elasticsearch Search Methods

        private async Task<SearchResultDto<ElasticDestination>> SearchDestinationsInElasticAsync(SearchRequestDto request)
        {
            var response = await _elasticClient.SearchAsync<ElasticDestination>(s => s
                .Index("destinations")
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(f => f
                            .Field(ff => ff.Name, 3.0)
                            .Field(ff => ff.Country, 2.0)
                            .Field(ff => ff.Description)
                            .Field(ff => ff.Tags)
                        )
                        .Query(request.Query)
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            return new SearchResultDto<ElasticDestination>
            {
                Results = response.Documents,
                Total = response.Total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        private async Task<SearchResultDto<ElasticFlight>> SearchFlightsInElasticAsync(SearchRequestDto request)
        {
            var response = await _elasticClient.SearchAsync<ElasticFlight>(s => s
                .Index("flights")
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(f => f
                            .Field(ff => ff.Airline, 2.0)
                            .Field(ff => ff.DepartureDestination)
                            .Field(ff => ff.ArrivalDestination)
                        )
                        .Query(request.Query)
                    )
                )
            );

            return new SearchResultDto<ElasticFlight>
            {
                Results = response.Documents,
                Total = response.Total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        private async Task<SearchResultDto<ElasticHotel>> SearchHotelsInElasticAsync(SearchRequestDto request)
        {
            var response = await _elasticClient.SearchAsync<ElasticHotel>(s => s
                .Index("hotels")
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(f => f
                            .Field(ff => ff.Name, 3.0)
                            .Field(ff => ff.Destination, 2.0)
                            .Field(ff => ff.Description)
                            .Field(ff => ff.Amenities)
                        )
                        .Query(request.Query)
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            return new SearchResultDto<ElasticHotel>
            {
                Results = response.Documents,
                Total = response.Total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        #endregion

        #region Database Query Methods

        private async Task<List<Destination>> GetDestinationsFromDatabaseAsync(string query)
        {
            return await _dbContext.Destinations
                .Where(d => d.Name.Contains(query) ||
                           d.Country.Contains(query) ||
                           d.Description.Contains(query))
                .ToListAsync();
        }

        private async Task<List<Flight>> GetFlightsFromDatabaseAsync(string query)
        {
            return await _dbContext.Flights
                .Include(f => f.DepartureDestination)
                .Include(f => f.ArrivalDestination)
                .Where(f => f.Airline.Contains(query) ||
                           f.DepartureDestination.Name.Contains(query) ||
                           f.ArrivalDestination.Name.Contains(query))
                .ToListAsync();
        }

        private async Task<List<Hotel>> GetHotelsFromDatabaseAsync(string query)
        {
            return await _dbContext.Hotels
                .Include(h => h.Destination)
                .Where(h => h.Name.Contains(query) ||
                           h.Destination.Name.Contains(query))
                .ToListAsync();
        }

        #endregion

        #region Entity Conversion Methods

        private ElasticDestination ConvertToElasticDestination(Destination destination)
        {
            return new ElasticDestination
            {
                Id = $"destination_{destination.DestinationId}",
                DestinationId = destination.DestinationId,
                Name = destination.Name,
                Country = destination.Country,
                Description = destination.Description,
                PopularKeywords = new[] { destination.Name, destination.Country },
                AverageHotelPrice = destination.Hotels?.Average(h => h.PricePerNight) ?? 0,
                PopularityScore = destination.Hotels?.Count ?? 0,
                Tags = new[] { destination.Country, "destination" }
            };
        }

        private ElasticFlight ConvertToElasticFlight(Flight flight)
        {
            var duration = (flight.ArrivalTime - flight.DepartureTime).TotalMinutes;

            return new ElasticFlight
            {
                Id = $"flight_{flight.FlightId}",
                FlightId = flight.FlightId,
                Airline = flight.Airline,
                DepartureDestination = flight.DepartureDestination?.Name,
                ArrivalDestination = flight.ArrivalDestination?.Name,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                Price = flight.Price,
                DurationMinutes = (int)duration,
                HasStopovers = false, // You can set this based on your business logic
                Amenities = new[] { "Standard" } // Default amenities
            };
        }

        private ElasticHotel ConvertToElasticHotel(Hotel hotel)
        {
            return new ElasticHotel
            {
                Id = $"hotel_{hotel.HotelId}",
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Destination = hotel.Destination?.Name,
                PricePerNight = hotel.PricePerNight,
                Rating = hotel.Rating,
                Amenities = new[] { "WiFi", "Parking" } // You can parse from hotel.Amenities if it exists
            };
        }

        #endregion

        #region Individual Index Operations

        public async Task IndexDestinationAsync(ElasticDestination destination)
        {
            var response = await _elasticClient.IndexAsync(destination, i => i
                .Index("destinations")
                .Id(destination.Id)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index destination: {Error}", response.DebugInformation);
            }
        }

        public async Task IndexFlightAsync(ElasticFlight flight)
        {
            var response = await _elasticClient.IndexAsync(flight, i => i
                .Index("flights")
                .Id(flight.Id)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index flight: {Error}", response.DebugInformation);
            }
        }

        public async Task IndexHotelAsync(ElasticHotel hotel)
        {
            var response = await _elasticClient.IndexAsync(hotel, i => i
                .Index("hotels")
                .Id(hotel.Id)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index hotel: {Error}", response.DebugInformation);
            }
        }

        #endregion

        #region Bulk Operations

        public async Task BulkIndexDestinationsAsync(IEnumerable<ElasticDestination> destinations)
        {
            var bulkDescriptor = new BulkDescriptor();
            foreach (var destination in destinations)
            {
                bulkDescriptor.Index<ElasticDestination>(x => x
                    .Index("destinations")
                    .Id(destination.Id)
                    .Document(destination)
                );
            }

            var response = await _elasticClient.BulkAsync(bulkDescriptor);
            if (!response.IsValid)
            {
                _logger.LogError("Bulk index destinations failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Successfully indexed {Count} destinations", destinations.Count());
            }
        }

        public async Task BulkIndexFlightsAsync(IEnumerable<ElasticFlight> flights)
        {
            var bulkDescriptor = new BulkDescriptor();
            foreach (var flight in flights)
            {
                bulkDescriptor.Index<ElasticFlight>(x => x
                    .Index("flights")
                    .Id(flight.Id)
                    .Document(flight)
                );
            }

            var response = await _elasticClient.BulkAsync(bulkDescriptor);
            if (!response.IsValid)
            {
                _logger.LogError("Bulk index flights failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Successfully indexed {Count} flights", flights.Count());
            }
        }

        public async Task BulkIndexHotelsAsync(IEnumerable<ElasticHotel> hotels)
        {
            var bulkDescriptor = new BulkDescriptor();
            foreach (var hotel in hotels)
            {
                bulkDescriptor.Index<ElasticHotel>(x => x
                    .Index("hotels")
                    .Id(hotel.Id)
                    .Document(hotel)
                );
            }

            var response = await _elasticClient.BulkAsync(bulkDescriptor);
            if (!response.IsValid)
            {
                _logger.LogError("Bulk index hotels failed: {Error}", response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Successfully indexed {Count} hotels", hotels.Count());
            }
        }

        #endregion

        #region Delete Operations

        public async Task DeleteDestinationAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<ElasticDestination>(id, d => d
                .Index("destinations")
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete destination: {Error}", response.DebugInformation);
            }
        }

        public async Task DeleteFlightAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<ElasticFlight>(id, d => d
                .Index("flights")
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete flight: {Error}", response.DebugInformation);
            }
        }

        public async Task DeleteHotelAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<ElasticHotel>(id, d => d
                .Index("hotels")
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete hotel: {Error}", response.DebugInformation);
            }
        }

        #endregion

        #region Autocomplete

        public async Task<IEnumerable<AutocompleteResultDto>> AutocompleteAsync(string query)
        {
            var destinationResponse = await _elasticClient.SearchAsync<ElasticDestination>(s => s
                .Index("destinations")
                .Size(3)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(query)
                        .Analyzer("autocomplete")
                    )
                )
            );

            var hotelResponse = await _elasticClient.SearchAsync<ElasticHotel>(s => s
                .Index("hotels")
                .Size(3)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(query)
                        .Analyzer("autocomplete")
                    )
                )
            );

            var results = new List<AutocompleteResultDto>();

            results.AddRange(destinationResponse.Documents.Select(d => new AutocompleteResultDto
            {
                Text = $"{d.Name}, {d.Country}",
                Type = "destination",
                Id = d.Id
            }));

            results.AddRange(hotelResponse.Documents.Select(h => new AutocompleteResultDto
            {
                Text = $"{h.Name}, {h.Destination}",
                Type = "hotel",
                Id = h.Id
            }));

            return results.OrderBy(r => r.Text).Take(5);
        }

        #endregion
    }
}