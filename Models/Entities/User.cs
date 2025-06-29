// Models/Entities/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TravelBookingApi.Models.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string? Username { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "User";

        // Token fields
        public string? AuthToken { get; set; }
        public DateTime? TokenExpiry { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Booking>? Bookings { get; set; }
        [JsonIgnore]
        public UserPreference UserPreference { get; set; }
    }
}