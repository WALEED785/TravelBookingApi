// Models/DTOs/UserDTOs.cs
using System.ComponentModel.DataAnnotations;

namespace TravelBookingApi.Models.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }

    public class UserRegisterDTO
    {
        [Required]
        public string? Username { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required, MinLength(6)]
        public string? Password { get; set; }

        public string? FullName { get; set; }

        public string? Role { get; set; }

    }

    public class UserLoginDTO
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    public class UserUpdateDTO
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
    }
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public DateTime? Expiry { get; set; }
    }
}