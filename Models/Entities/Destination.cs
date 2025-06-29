using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Models.Entities
{
    public class Destination
    {
        [Key]
        public int DestinationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Hotel>? Hotels { get; set; }
        [JsonIgnore]
        public ICollection<Flight>? DepartureFlights { get; set; }
        [JsonIgnore]
        public ICollection<Flight>? ArrivalFlights { get; set; }
    }
}