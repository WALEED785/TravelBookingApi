using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TravelBookingApi.Models.Entities
{
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [ForeignKey("Destination")]
        public int DestinationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PricePerNight { get; set; }

        [Column(TypeName = "decimal(3, 1)")]
        public decimal? Rating { get; set; }

        [MaxLength(500)]
        public string? Amenities { get; set; }

        // Navigation properties
        [JsonIgnore]
        public Destination? Destination { get; set; }
        [JsonIgnore]
        public ICollection<Booking>? Bookings { get; set; }
    }
}