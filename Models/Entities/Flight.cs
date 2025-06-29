using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TravelBookingApi.Models.Entities
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Airline { get; set; }

        [ForeignKey("DepartureDestination")]
        public int DepartureDestinationId { get; set; }

        [ForeignKey("ArrivalDestination")]
        public int ArrivalDestinationId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        // Navigation properties
        [JsonIgnore]
        public Destination? DepartureDestination { get; set; }
        [JsonIgnore]
        public Destination? ArrivalDestination { get; set; }
        [JsonIgnore]
        public ICollection<Booking>? Bookings { get; set; }
    }
}