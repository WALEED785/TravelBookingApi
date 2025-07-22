using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Services.AISearch
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticsearchService> _logger;

        public ElasticsearchService(IElasticClient elasticClient, ILogger<ElasticsearchService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<bool> CreateIndicesAsync()
        {
            try
            {
                // Check if Elasticsearch is available
                var pingResponse = await _elasticClient.PingAsync();
                if (!pingResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch is not available: {Error}", pingResponse.DebugInformation);
                    return false;
                }

                // Check if indices already exist and delete them (for testing)
                var existingIndices = new[] { "destinations", "flights", "hotels" };
                foreach (var index in existingIndices)
                {
                    var existsResponse = await _elasticClient.Indices.ExistsAsync(index);
                    if (existsResponse.Exists)
                    {
                        _logger.LogInformation("Deleting existing index: {Index}", index);
                        await _elasticClient.Indices.DeleteAsync(index);
                    }
                }

                // Create destination index
                var destinationIndexResponse = await _elasticClient.Indices.CreateAsync("destinations", c => c
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
                    .Map<ElasticDestination>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Name)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.Country)
                                .Analyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.Description)
                                .Analyzer("standard")
                            )
                            .Keyword(k => k
                                .Name(n => n.Tags)
                            )
                        )
                    )
                );

                if (!destinationIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create destinations index: {Error}", destinationIndexResponse.DebugInformation);
                    return false;
                }

                // Create flights index
                var flightIndexResponse = await _elasticClient.Indices.CreateAsync("flights", c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                    )
                    .Map<ElasticFlight>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Airline)
                                .Analyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.DepartureDestination)
                                .Analyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.ArrivalDestination)
                                .Analyzer("standard")
                            )
                            .Date(d => d
                                .Name(n => n.DepartureTime)
                            )
                            .Date(d => d
                                .Name(n => n.ArrivalTime)
                            )
                        )
                    )
                );

                if (!flightIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create flights index: {Error}", flightIndexResponse.DebugInformation);
                    return false;
                }

                // Create hotels index
                var hotelIndexResponse = await _elasticClient.Indices.CreateAsync("hotels", c => c
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
                    .Map<ElasticHotel>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Name)
                                .Analyzer("autocomplete")
                                .SearchAnalyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.Destination)
                                .Analyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.Description)
                                .Analyzer("standard")
                            )
                            .Keyword(k => k
                                .Name(n => n.Amenities)
                            )
                        )
                    )
                );

                if (!hotelIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create hotels index: {Error}", hotelIndexResponse.DebugInformation);
                    return false;
                }

                _logger.LogInformation("All Elasticsearch indices created successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Elasticsearch indices");
                return false;
            }
        }

        public async Task IndexDestinationAsync(ElasticDestination destination)
        {
            var response = await _elasticClient.IndexAsync(destination, i => i
                .Index("destinations")
                .Id(destination.Id)
            );

            if (!response.IsValid)
            {
                _logger.LogError(response.DebugInformation);
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
                _logger.LogError(response.DebugInformation);
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
                _logger.LogError(response.DebugInformation);
            }
        }

        public async Task DeleteDestinationAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<ElasticDestination>(id, d => d
                .Index("destinations")
            );

            if (!response.IsValid)
            {
                _logger.LogError(response.DebugInformation);
            }
        }

        public async Task DeleteFlightAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<ElasticFlight>(id, d => d
                .Index("flights")
            );

            if (!response.IsValid)
            {
                _logger.LogError(response.DebugInformation);
            }
        }

        public async Task DeleteHotelAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<ElasticHotel>(id, d => d
                .Index("hotels")
            );

            if (!response.IsValid)
            {
                _logger.LogError(response.DebugInformation);
            }
        }

        public async Task<SearchResultDto<ElasticDestination>> SearchDestinationsAsync(SearchRequestDto request)
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
                .Sort(GetSortDescriptor<ElasticDestination>(request))
            );

            return new SearchResultDto<ElasticDestination>
            {
                Results = response.Documents,
                Total = response.Total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<SearchResultDto<ElasticFlight>> SearchFlightsAsync(SearchRequestDto request)
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
                    ) && +q
                    .DateRange(r => r
                        .Field(f => f.DepartureTime)
                        .GreaterThanOrEquals(DateTime.UtcNow)
                    )
                )
                .Sort(GetSortDescriptor<ElasticFlight>(request))
            );

            return new SearchResultDto<ElasticFlight>
            {
                Results = response.Documents,
                Total = response.Total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<SearchResultDto<ElasticHotel>> SearchHotelsAsync(SearchRequestDto request)
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
                .Sort(GetSortDescriptor<ElasticHotel>(request))
            );

            return new SearchResultDto<ElasticHotel>
            {
                Results = response.Documents,
                Total = response.Total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<IEnumerable<AutocompleteResultDto>> AutocompleteAsync(string query)
        {
            var destinationResponse = await _elasticClient.SearchAsync<ElasticDestination>(s => s
                .Index("destinations")
                .Size(5)
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
                .Size(5)
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
                _logger.LogError(response.DebugInformation);
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
                _logger.LogError(response.DebugInformation);
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
                _logger.LogError(response.DebugInformation);
            }
        }

        private Func<SortDescriptor<T>, IPromise<IList<ISort>>> GetSortDescriptor<T>(SearchRequestDto request) where T : class
        {
            return sort =>
            {
                if (string.IsNullOrEmpty(request.SortBy))
                {
                    sort.Descending(SortSpecialField.Score);
                }
                else
                {
                    var field = request.SortBy.ToLower();
                    if (request.SortDescending)
                    {
                        sort.Descending(field);
                    }
                    else
                    {
                        sort.Ascending(field);
                    }
                }
                return sort;
            };
        }
    }
}