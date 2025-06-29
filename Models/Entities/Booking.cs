using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Models.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Hotel")]
        public int? HotelId { get; set; }

        [ForeignKey("Flight")]
        public int? FlightId { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed";

        // Navigation properties
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Hotel? Hotel { get; set; }
        [JsonIgnore]
        public Flight? Flight { get; set; }
    }
}