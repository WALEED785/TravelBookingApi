using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Models.Entities
{
    public class UserPreference
    {
        [Key]
        public int PreferenceId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? PreferredDestinationIds { get; set; } // Comma-separated (e.g., "1,2,3")

        [MaxLength(50)]
        public string BudgetRange { get; set; } // "Low", "Medium", "High"

        [MaxLength(500)]
        public string? PreferredAmenities { get; set; } // "WiFi,Pool,Gym"

        // Navigation property
        [JsonIgnore]
        public User? User { get; set; }
    }
}