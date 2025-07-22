using Nest;
using System;

namespace TravelBookingApi.Models.Entities
{
    // Destination Index
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class ElasticDestination
    {
        [Keyword]
        public string? Id { get; set; } // "destination_{DestinationId}"

        [Number(Name = "destination_id")]
        public int DestinationId { get; set; }

        [Text(Name = "name", Analyzer = "autocomplete")]
        public string? Name { get; set; }

        [Keyword]
        public string? Country { get; set; }

        [Text]
        public string? Description { get; set; }

        [Keyword]
        public string?[] PopularKeywords { get; set; } = [];

        [Number(NumberType.Double)]
        public decimal AverageHotelPrice { get; set; }

        [Number(NumberType.Integer)]
        public int PopularityScore { get; set; }

        [Keyword]
        public string?[] Tags { get; set; } = [];
    }

    // Flight Index
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class ElasticFlight
    {
        [Keyword]
        public string? Id { get; set; } // "flight_{FlightId}"

        [Number(Name = "flight_id")]
        public int FlightId { get; set; }

        [Text(Name = "airline", Analyzer = "standard")]
        public string? Airline { get; set; }

        [Keyword]
        public string? DepartureDestination { get; set; } // Use DepartureDestination.Name

        [Keyword]
        public string? ArrivalDestination { get; set; } // Use ArrivalDestination.Name

        [Date]
        public DateTime DepartureTime { get; set; }

        [Date]
        public DateTime ArrivalTime { get; set; }

        [Number(NumberType.Double)]
        public decimal Price { get; set; }

        [Number(NumberType.Integer)]
        public int DurationMinutes { get; set; } // Derived from time difference
        [Keyword]
        public string? FlightClass { get; set; } // optional if applicable
        [Boolean]
        public bool HasStopovers { get; set; }
        [Keyword]
        public string?[] Amenities { get; set; } = []; // if flight amenities exist
    }


    [ElasticsearchType(IdProperty = nameof(Id))]
    public class ElasticHotel
    {
        [Keyword]
        public string? Id { get; set; } // "hotel_{HotelId}"
        [Number(Name = "hotel_id")]
        public int HotelId { get; set; }
        [Text(Name = "name", Analyzer = "autocomplete")]
        public string? Name { get; set; }
        [Keyword]
        public string? Destination { get; set; } // Destination.Name
        [Number(NumberType.Double)]
        public decimal PricePerNight { get; set; }
        [Number(NumberType.Double)]
        public decimal? Rating { get; set; }
        [Keyword]
        public string?[] Amenities { get; set; } = [];
        [Text]
        public string? Description { get; set; }
    }


    // Search DTOs
    public class SearchRequestDto
    {
        public string? Query { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string?[] Filters { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class SearchResultDto<T>
    {
        public IEnumerable<T> Results { get; set; }
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class AutocompleteResultDto
    {
        public string? Text { get; set; }
        public string? Type { get; set; } // "destination", "hotel", "flight"
        public string? Id { get; set; }
    }
}